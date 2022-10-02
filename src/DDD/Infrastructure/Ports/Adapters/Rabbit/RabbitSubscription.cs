using RabbitMQ.Client;

namespace DDD.Infrastructure.Ports.Adapters.Rabbit
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
