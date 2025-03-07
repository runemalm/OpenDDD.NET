using OpenDDD.Infrastructure.Events.Base;

namespace OpenDDD.Infrastructure.Events.InMemory
{
    public class InMemorySubscription : Subscription<InMemorySubscription>
    {
        public Func<string, CancellationToken, Task> MessageHandler { get; }

        public InMemorySubscription(string topic, string consumerGroup, Func<string, CancellationToken, Task> messageHandler)
            : base(topic, consumerGroup, null)
        {
            if (string.IsNullOrWhiteSpace(topic))
                throw new ArgumentException("Topic cannot be null or empty.", nameof(topic));

            if (string.IsNullOrWhiteSpace(consumerGroup))
                throw new ArgumentException("Consumer group cannot be null or empty.", nameof(consumerGroup));

            MessageHandler = messageHandler;
        }

        public override ValueTask DisposeAsync()
        {
            return ValueTask.CompletedTask;
        }
    }
}
