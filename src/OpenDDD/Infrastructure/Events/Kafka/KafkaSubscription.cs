using OpenDDD.Infrastructure.Events.Base;
using OpenDDD.Infrastructure.Events.Kafka.Factories;

namespace OpenDDD.Infrastructure.Events.Kafka
{
    public class KafkaSubscription : Subscription<KafkaConsumer>
    {
        public KafkaSubscription(string topic, string consumerGroup, KafkaConsumer consumer)
            : base(topic, consumerGroup, consumer) { }
        
        public override async ValueTask DisposeAsync()
        {
            await Consumer.DisposeAsync();
        }

        public void PrintDebugInfo()
        {
            var assignments = Consumer._consumer.Assignment;
            Console.WriteLine($"Consumer assigned partitions: {string.Join(", ", assignments.Select(a => a.Partition.Value))}");
        }
    }
}
