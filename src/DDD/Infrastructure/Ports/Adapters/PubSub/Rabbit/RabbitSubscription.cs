using DDD.Infrastructure.Ports.PubSub;
using RabbitMQ.Client;

namespace DDD.Infrastructure.Ports.Adapters.PubSub.Rabbit
{
	public class RabbitSubscription : Subscription
	{
		public IModel Channel;
		public string ConsumerTag;

		public RabbitSubscription(IEventListener listener) : base(listener)
		{
			
		}
	}
}
