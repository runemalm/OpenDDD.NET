using System.Collections.Generic;
using System.Threading.Tasks;
using DDD.Domain;

namespace DDD.Infrastructure.Ports
{
	public interface IEventAdapter
	{
		public int MaxDeliveryRetries { get; }

		Task StartAsync();
		Task StopAsync();
		Task<Subscription> SubscribeAsync(IEventListener listener);
		Task UnsubscribeAsync(IEventListener listener);
		Task PublishAsync(IEvent theEvent);
		Task FlushAsync(OutboxEvent outboxEvent);
		bool HasPublished(IEvent theEvent);
		bool HasFlushed(IEvent theEvent);
		Task<IEnumerable<IEvent>> GetPublishedAsync(ActionId actionId);
		Task AckAsync(IPubSubMessage message);
		string GetContext();
		int GetPublishedCount();
		int GetFlushedCount();
	}
}
