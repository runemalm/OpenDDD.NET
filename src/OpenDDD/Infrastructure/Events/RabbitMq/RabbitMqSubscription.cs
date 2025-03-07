using OpenDDD.Infrastructure.Events.Base;

namespace OpenDDD.Infrastructure.Events.RabbitMq
{
    public class RabbitMqSubscription : Subscription<RabbitMqCustomAsyncConsumer>
    {
        public RabbitMqSubscription(string topic, string consumerGroup, RabbitMqCustomAsyncConsumer consumer) 
            : base(topic, consumerGroup, consumer) { }

        public override async ValueTask DisposeAsync()
        {
            await Consumer.StopConsumingAsync(CancellationToken.None);
            await Consumer.DisposeAsync();
        }
    }
}
