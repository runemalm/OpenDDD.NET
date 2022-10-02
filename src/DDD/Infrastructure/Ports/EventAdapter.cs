using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DDD.Domain;
using DDD.Infrastructure.Exceptions;
using DDD.Logging;
using KellermanSoftware.CompareNetObjects;
using Namotion.Reflection;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;

namespace DDD.Infrastructure.Ports
{
	public abstract class EventAdapter<TSub> : IEventAdapter where TSub : Subscription
	{
		protected string _context;
		protected string _client;
		private ICollection<TSub> _subscriptions = new List<TSub>();
		private IDictionary<ActionId, ICollection<IEvent>> _published = new Dictionary<ActionId, ICollection<IEvent>>();
		private IDictionary<ActionId, ICollection<OutboxEvent>> _flushed = new Dictionary<ActionId, ICollection<OutboxEvent>>();
		public bool IsStarted = false;
		public bool IsStopped => !IsStarted;
		public bool IsStopping = false;
		protected ILogger _logger;
		protected IMonitoringPort _monitoringAdapter;
		public int MaxDeliveryRetries { get; }

		public abstract Task StartAsync();
		public abstract Task StopAsync();
		public abstract Task<Subscription> SubscribeAsync(IEventListener listener);
		public abstract Task UnsubscribeAsync(IEventListener listener);
		public abstract Task AckAsync(IPubSubMessage message);

		protected readonly JsonSerializerSettings _jsonSerializerSettings =
			new JsonSerializerSettings
			{
				ContractResolver = new CamelCasePropertyNamesContractResolver(),
				Converters = new List<JsonConverter>()
				{
					new StringEnumConverter
					{
						AllowIntegerValues = false,
						NamingStrategy = new DefaultNamingStrategy()
					}
				},
				DateFormatString = "yyyy-MM-dd'T'HH:mm:ss.fffK",
				NullValueHandling = NullValueHandling.Ignore,
				Formatting = Formatting.Indented
			};

		public EventAdapter(
			string context,
			string client,
			int maxDeliveryRetries,
			ILogger logger,
			IMonitoringPort monitoringAdapter)
		{
			_context = context;
			_client = client;
			MaxDeliveryRetries = maxDeliveryRetries;
			_logger = logger;
			_monitoringAdapter = monitoringAdapter;
		}

		protected void AddSubscription(TSub subscription)
		{
			_subscriptions.Add(subscription);
		}

		protected TSub GetSubscription(IEventListener listener)
			=> GetSubscription(listener.ListensTo, listener.ListensToVersion);

		protected TSub GetSubscription(string eventName, DomainModelVersion domainModelVersion)
		{
			foreach (var subscription in _subscriptions)
				if (subscription.EventName == eventName &&
					subscription.DomainModelVersion == domainModelVersion)
					return subscription;
			return null;
		}

		protected IEnumerable<TSub> GetSubscriptions()
		{
			return _subscriptions;
		}
		
		protected void RemoveSubscription(TSub subscription)
		{
			_subscriptions = 
				_subscriptions
					.Where(s =>
						!(s.EventName == subscription.EventName && s.DomainModelVersion == subscription.DomainModelVersion))
					.ToList();
		}

		public virtual Task PublishAsync(IEvent theEvent)
		{
			/*
			 * Add event to the outbox.
			 */
			if (!_published.ContainsKey(theEvent.Header.ActionId))
				_published.Add(theEvent.Header.ActionId, new List<IEvent>());
			_published[theEvent.Header.ActionId].Add(theEvent);
			return Task.CompletedTask;
		}
		
		public bool HasPublished(IEvent theEvent)
		{
			CompareLogic compareLogic = new CompareLogic();
			compareLogic.Config.MaxDifferences = 1;
			compareLogic.Config.MaxMillisecondsDateDifference = 999;
			compareLogic.Config.IgnoreCollectionOrder = true;
			compareLogic.Config.MembersToIgnore.Add("EventId");
			compareLogic.Config.MembersToIgnore.Add("ActionId");

			foreach (var coll in _published.Values)
			{
				foreach (var e in coll)
				{
					var result = compareLogic.Compare(theEvent, e);
					if (result.AreEqual)
						return true;
				}
			}

			return false;
		}

		public bool HasFlushed(IEvent theEvent)
		{
			foreach (var outboxEvents in _flushed.Values)
			{
				foreach (var o in outboxEvents)
				{
					if (o.EventName == theEvent.Header.Name)
					{
						var theEventObject = (JObject)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(theEvent));
						var flushedEventObject = (JObject)JsonConvert.DeserializeObject(o.JsonPayload);

						((JObject)theEventObject["header"]).Remove("eventId");
						((JObject)theEventObject["header"]).Remove("actionId");
						((JObject)theEventObject["header"]).Remove("occuredAt");
						((JObject)flushedEventObject["header"]).Remove("eventId");
						((JObject)flushedEventObject["header"]).Remove("actionId");
						((JObject)flushedEventObject["header"]).Remove("occuredAt");

						var isEqual = JObject.DeepEquals(theEventObject, flushedEventObject);

						if (isEqual)
							return true;						
					}
				}
			}

			return false;
		}

		public Task<IEnumerable<IEvent>> GetPublishedAsync(ActionId actionId)
		{
			ICollection<IEvent> events;
			if (!_published.TryGetValue(actionId, out events))
				events = new List<IEvent>();
			return Task.FromResult(events.AsEnumerable());
		}

		public virtual Task FlushAsync(OutboxEvent outboxEvent)
		{
			RemoveFromPublished(outboxEvent);
			AddToFlushed(outboxEvent);
			return Task.CompletedTask;
		}
		
		private void RemoveFromPublished(OutboxEvent outboxEvent)
		{
			if (_published.ContainsKey(outboxEvent.ActionId))
			{
				_published[outboxEvent.ActionId] =
					_published[outboxEvent.ActionId].Where(
						o => 
							o.Header.EventId != outboxEvent.EventId).ToList();
			}
		}
		
		private void AddToFlushed(OutboxEvent outboxEvent)
		{
			if (!_flushed.ContainsKey(outboxEvent.ActionId))
				_flushed.Add(outboxEvent.ActionId, new List<OutboxEvent>());
			_flushed[outboxEvent.ActionId].Add(outboxEvent);
		}

		private T PropertyInOutboxEvent<T>(string propertyName, OutboxEvent outboxEvent)
		{
			var theEvent = JsonConvert.DeserializeObject(outboxEvent.JsonPayload);
			return theEvent.TryGetPropertyValue<T>(propertyName);
		}
		
		private string EventNameFromBody(string body)
		{
			throw new NotImplementedException();
		}

		public string TopicForEvent(IEvent theEvent)
			=> TopicForEvent(theEvent.Header.Name, theEvent.Header.DomainModelVersion);

		public string TopicForEvent(IEventListener listener)
			=> TopicForEvent(listener.ListensTo, listener.ListensToVersion);

		public string TopicForEvent(string eventName, DomainModelVersion domainModelVersion)
			=> $"{_context}-{eventName}-{domainModelVersion.ToStringWithWildcardBuild()}";

		public string TopicSubscriptionForEvent(IEvent theEvent)
			=> TopicSubscriptionForEvent(theEvent.Header.Name, theEvent.Header.DomainModelVersion);

		public string TopicSubscriptionForEvent(IEventListener listener)
			=> TopicSubscriptionForEvent(listener.ListensTo, listener.ListensToVersion);

		public string TopicSubscriptionForEvent(string eventName, DomainModelVersion domainModelVersion)
			=> $"{TopicForEvent(eventName, domainModelVersion)}-{_client}";

		public string GetContext()
			=> _context;

		public int GetPublishedCount()
			=> _published.Count;
		
		public int GetFlushedCount()
			=> _flushed.Count;
	}
}
