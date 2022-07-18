using System.Threading.Tasks;
using DDD.Domain;
using DDD.Infrastructure.Ports.Adapters.Memory.Exceptions;
using DDD.Logging;
using Newtonsoft.Json;

namespace DDD.Infrastructure.Ports.Adapters.Memory
{
	public class MemoryEventAdapter : EventAdapter
	{
		public MemoryEventAdapter(
			string topic,
			string client,
			bool listenerAcksRequired,
			bool publisherAcksRequired,
			ILogger logger,
			IMonitoringPort monitoringAdapter)
			: base(topic, client, listenerAcksRequired, publisherAcksRequired, logger, monitoringAdapter)
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
			var subscription = base.SubscribeAsync(listener);

			// Start consuming
			// TODO: Implement consumption..

			return subscription;
		}

		public async override Task AckAsync(IPubSubMessage message)
		{
			if (!(message is MemoryMessage))
				throw new MemoryException(
					"Expected IPubSubMessage to be a MemoryMessage. " +
					"Something must be wrong with the implementation.");
			else
			{
				var memoryMessage = (MemoryMessage)message;
				// TODO: Ack..
				await Task.Yield();
			}
		}

		public override Task FlushAsync(OutboxEvent outboxEvent)
		{
			var message = outboxEvent.JsonPayload;

			// TODO: Send on bus..

			base.FlushAsync(outboxEvent);

			return Task.CompletedTask;
		}
	}
}
