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
using JsonSerializer = System.Text.Json.JsonSerializer;

// using SystemJsonSerializer = System.Text.Json.JsonSerializer;

namespace DDD.Infrastructure.Ports
{
	public abstract class EventAdapter : IEventAdapter
	{
		protected string _context;
		protected string _client;
		private ICollection<Subscription> _subscriptions = new List<Subscription>();
		private IDictionary<ActionId, ICollection<IEvent>> _published = new Dictionary<ActionId, ICollection<IEvent>>();
		private IDictionary<ActionId, ICollection<OutboxEvent>> _flushed = new Dictionary<ActionId, ICollection<OutboxEvent>>();
		public bool IsStarted = false;
		public bool IsStopped => !IsStarted;
		public bool IsStopping = false;
		protected ILogger _logger;
		protected IMonitoringPort _monitoringAdapter;
		private bool _listenerAcksRequired;
		private bool _publisherAcksRequired;

		public abstract Task StartAsync();
		public abstract Task StopAsync();

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
			bool listenerAcksRequired,
			bool publisherAcksRequired,
			ILogger logger,
			IMonitoringPort monitoringAdapter)
		{
			_context = context;
			_client = client;
			_listenerAcksRequired = listenerAcksRequired;
			_publisherAcksRequired = publisherAcksRequired;
			_logger = logger;
			_monitoringAdapter = monitoringAdapter;
		}

		public abstract Task AckAsync(IPubSubMessage message);

		protected async Task Handle(IPubSubMessage message, IEventListener listener)
		{
			if (_listenerAcksRequired)
			{
				await listener.React(message);
				await AckAsync(message);
			}
			else
			{
				await AckAsync(message);
				await listener.React(message);
			}
		}

		public virtual Task<Subscription> SubscribeAsync(IEventListener listener)
		{
			if (!IsStarted)
				throw new PubSubException(
					"Event adapter must be started before you can subscribe.");

			var subscription = GetSubscription(listener);

			if (subscription != null)
				throw new Exception(
					"Can't subscribe, we have already subscribed for that " +
					"event in this event adapter. Only one subscription " +
					"per event and adapter is allowed.");

			subscription = CreateSubscription(listener);

			AddSubscription(subscription);

			return Task.FromResult(subscription);
		}

		private Subscription CreateSubscription(IEventListener listener)
		{
			var subscription = new Subscription(listener);
			return subscription;
		}

		private void AddSubscription(Subscription subscription)
		{
			_subscriptions.Add(subscription);
		}

		private Subscription GetSubscription(IEventListener listener)
			=> GetSubscription(listener.ListensTo, listener.ListensToVersion);

		private Subscription GetSubscription(string eventName, IDomainModelVersion domainModelVersion)
		{
			foreach (var subscription in _subscriptions)
				if (subscription.EventName == eventName &&
					subscription.DomainModelVersion == domainModelVersion)
					return subscription;
			return null;
		}

		public IEnumerable<Subscription> GetSubscriptions()
		{
			return _subscriptions;
		}

		public virtual Task PublishAsync(IEvent theEvent)
		{
			/*
			 * Add event to the outbox.
			 */
			if (!_published.ContainsKey(theEvent.ActionId))
				_published.Add(theEvent.ActionId, new List<IEvent>());
			_published[theEvent.ActionId].Add(theEvent);
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
					if (o.EventName == theEvent.EventName)
					{
						var theEventObject = (JObject)JsonConvert.DeserializeObject(JsonConvert.SerializeObject(theEvent));
						var flushedEventObject = (JObject)JsonConvert.DeserializeObject(o.JsonPayload);

						theEventObject.Remove("eventId");
						theEventObject.Remove("actionId");
						theEventObject.Remove("occuredAt");
						flushedEventObject.Remove("eventId");
						flushedEventObject.Remove("actionId");
						flushedEventObject.Remove("occuredAt");
					
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
			var hasPublished =
				_published.ContainsKey(outboxEvent.ActionId) &&
				_published[outboxEvent.ActionId].Any(o => o.EventId == outboxEvent.EventId);

			if (!hasPublished)
			{
				var eventName = PropertyInOutboxEvent<string>("EventName", outboxEvent);
				throw new PubSubException(
					$"Can't flush event '{eventName}' because it hasn't been published.");
			}

			RemoveFromPublished(outboxEvent);
			AddToFlushed(outboxEvent);

			return Task.CompletedTask;
		}
		
		private void RemoveFromPublished(OutboxEvent outboxEvent)
		{
			if (_published.ContainsKey(outboxEvent.ActionId))
			{
				_published[outboxEvent.ActionId] =
					_published[outboxEvent.ActionId].Where(o => o.EventId != outboxEvent.EventId).ToList();
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

		public string SerializeEvent(IEvent theEvent)
			=> JsonConvert.SerializeObject(theEvent, _jsonSerializerSettings);

		public string TopicForEvent(IEvent theEvent)
			=> TopicForEvent(theEvent.EventName, theEvent.DomainModelVersion);

		public string TopicForEvent(IEventListener listener)
			=> TopicForEvent(listener.ListensTo, listener.ListensToVersion);

		public string TopicForEvent(string eventName, IDomainModelVersion domainModelVersion)
			=> $"{_context}-{eventName}-{domainModelVersion}";

		public string TopicSubscriptionForEvent(IEvent theEvent)
			=> TopicSubscriptionForEvent(theEvent.EventName, theEvent.DomainModelVersion);

		public string TopicSubscriptionForEvent(IEventListener listener)
			=> TopicSubscriptionForEvent(listener.ListensTo, listener.ListensToVersion);

		public string TopicSubscriptionForEvent(string eventName, IDomainModelVersion domainModelVersion)
			=> $"{TopicForEvent(eventName, domainModelVersion)}-{_client}";

		public string GetContext()
			=> _context;

		public int GetPublishedCount()
			=> _published.Count;
		
		public int GetFlushedCount()
			=> _flushed.Count;
	}
}
