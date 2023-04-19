using System;
using System.Threading.Tasks;
using DDD.Infrastructure.Ports.Adapters.Common.Exceptions;
using DDD.Infrastructure.Ports.Adapters.Common.Translation.Converters;
using DDD.Infrastructure.Ports.Monitoring;
using DDD.Infrastructure.Ports.PubSub;
using DDD.Logging;

namespace DDD.Infrastructure.Ports.Adapters.PubSub.ServiceBus
{
	public class ServiceBusEventAdapter : EventAdapter<Subscription>
	{
		private string _connString;
		private string _subName;

		public ServiceBusEventAdapter(
			string context,
			string client,
			int maxDeliveryRetries,
			string connString,
			string subName,
			ILogger logger,
			IMonitoringPort monitoringAdapter,
			SerializerSettings serializerSettings) :
			base(
				context,
				client,
				maxDeliveryRetries,
				logger,
				monitoringAdapter,
				serializerSettings)
		{
			_connString = connString;
			_subName = subName;
		}

		public override void Start()
		{
			throw new NotImplementedException();
		}

		public override Task StartAsync()
		{
			throw new NotImplementedException();
		}

		public override void Stop()
		{
			throw new NotImplementedException();
		}

		public override Task StopAsync()
		{
			throw new NotImplementedException();
		}

		public override Subscription Subscribe(IEventListener listener)
		{
			var subscription = new Subscription(listener);
			AddSubscription(subscription);

			// TODO: Start listening for event..

			throw new NotImplementedException();
		}

		public override Task<Subscription> SubscribeAsync(IEventListener listener)
			=> Task.FromResult(Subscribe(listener));
		
		public override void Unsubscribe(IEventListener listener)
		{
			var subscription = GetSubscription(listener);
			RemoveSubscription(subscription);
		}
		
		public override Task UnsubscribeAsync(IEventListener listener)
		{
			Unsubscribe(listener);
			return Task.CompletedTask;
		}

		public override Task AckAsync(IPubSubMessage message)
		{
			if (!(message is ServiceBusMessage))
				throw new ServiceBusException(
					"Expected IPubSubMessage to be a ServiceBusMessage. " +
					"Something must be wrong with the implementation.");
			else
			{
				throw new System.NotImplementedException();

				var sbMessage = (ServiceBusMessage)message;
				// ...
			}
		}

		public override Task FlushAsync(OutboxEvent outboxEvent)
		{
			throw new NotImplementedException();

			// TODO: Actually flush event (send it on bus)

			base.FlushAsync(outboxEvent);

			return Task.CompletedTask;
		}
	}
}
