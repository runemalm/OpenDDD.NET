namespace OpenDDD.Infrastructure.Events.Base
{
    public class Subscription<TConsumer> : ISubscription where TConsumer : IAsyncDisposable
    {
        public string Id { get; } = Guid.NewGuid().ToString();
        public string Topic { get; }
        public string ConsumerGroup { get; }
        public TConsumer? Consumer { get; }

        public Subscription(string topic, string consumerGroup, TConsumer? consumer)
        {
            Topic = topic ?? throw new ArgumentException(nameof(topic));
            ConsumerGroup = consumerGroup ?? throw new ArgumentException(nameof(consumerGroup));
            Consumer = consumer;
        }

        public virtual async ValueTask DisposeAsync()
        {
            await Consumer.DisposeAsync();
            await ValueTask.CompletedTask;
        }
    }
}
