using System;
using System.Threading.Tasks;
using DDD.Domain;
using DDD.Infrastructure.Ports.Adapters.ServiceBus.Exceptions;
using DDD.Logging;

namespace DDD.Infrastructure.Ports.Adapters.ServiceBus
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
			IMonitoringPort monitoringAdapter) :
			base(
				context,
				client,
				maxDeliveryRetries,
				logger,
				monitoringAdapter)
		{
			_connString = connString;
			_subName = subName;
		}

		public override Task StartAsync()
		{
			throw new System.NotImplementedException();
		}

		public override Task StopAsync()
		{
			throw new System.NotImplementedException();
		}

		public override async Task<Subscription> SubscribeAsync(IEventListener listener)
		{
			var subscription = new Subscription(listener);
			AddSubscription(subscription);

			// TODO: Start listening for event..

			throw new NotImplementedException();

			// return Task.FromResult(subscription);
		}
		
		public override async Task UnsubscribeAsync(IEventListener listener)
		{
			var subscription = GetSubscription(listener);
			RemoveSubscription(subscription);
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
