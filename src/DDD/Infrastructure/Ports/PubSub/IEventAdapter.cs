using System.Collections.Generic;
using System.Threading.Tasks;
using DDD.Application;
using DDD.Domain.Model.BuildingBlocks.Event;

namespace DDD.Infrastructure.Ports.PubSub
{
	public interface IEventAdapter
	{
		public int MaxDeliveryRetries { get; }

		void Start();
		Task StartAsync();
		void Stop();
		Task StopAsync();
		Subscription Subscribe(IEventListener listener);
		Task<Subscription> SubscribeAsync(IEventListener listener);
		void Unsubscribe(IEventListener listener);
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
