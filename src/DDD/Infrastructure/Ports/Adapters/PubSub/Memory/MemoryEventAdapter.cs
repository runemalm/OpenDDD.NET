using System.Threading.Tasks;
using DDD.Infrastructure.Ports.Adapters.Common.Exceptions;
using DDD.Infrastructure.Ports.Monitoring;
using DDD.Infrastructure.Ports.PubSub;
using DDD.Logging;

namespace DDD.Infrastructure.Ports.Adapters.PubSub.Memory
{
	public class MemoryEventAdapter : EventAdapter<Subscription>
	{
		public MemoryEventAdapter(
			string topic,
			string client,
			int maxDeliveryRetries,
			ILogger logger,
			IMonitoringPort monitoringAdapter)
			: base(
				topic, 
				client, 
				maxDeliveryRetries,
				logger, 
				monitoringAdapter)
		{
			
		}

		public override async Task StartAsync()
		{
			IsStarted = true;
			await Task.Yield();
		}

		public override async Task StopAsync()
		{
			IsStarted = false;
			await Task.Yield();
		}

		public override Task<Subscription> SubscribeAsync(IEventListener listener)
		{
			var subscription = new Subscription(listener);
			AddSubscription(subscription);
			return Task.FromResult(subscription);
		}
		
		public override async Task UnsubscribeAsync(IEventListener listener)
		{
			var subscription = GetSubscription(listener);
			RemoveSubscription(subscription);
		}

		public override async Task AckAsync(IPubSubMessage message)
		{
			if (!(message is MemoryMessage))
			{
				throw new MemoryException(
					"Expected IPubSubMessage to be a MemoryMessage. " +
					"Something must be wrong with the implementation.");
			}
			else
			{
				var memoryMessage = (MemoryMessage)message;
				
				// Need no ack here since memory based event adapter will always succeed with delivery..
				
				await Task.Yield();
			}
		}

		public override async Task FlushAsync(OutboxEvent outboxEvent)
		{
			if (!IsStarted)
				throw new MemoryException("Can't flush event, memory event adapter is not started.");

			var message = new MemoryMessage(outboxEvent.JsonPayload);

			foreach (var sub in GetSubscriptions())
			{
				if (sub.EventName == outboxEvent.EventName)
				{
					if (sub.DomainModelVersion.ToStringWithWildcardBuild() ==
					    outboxEvent.DomainModelVersion.ToStringWithWildcardBuild())
					{
						await sub.Listener.Handle(message);
					}
				}
			}

			base.FlushAsync(outboxEvent);
		}
	}
}
