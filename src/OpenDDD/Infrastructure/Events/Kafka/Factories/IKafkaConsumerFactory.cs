namespace OpenDDD.Infrastructure.Events.Kafka.Factories
{
    public interface IKafkaConsumerFactory
    {
        KafkaConsumer Create(string consumerGroup);
    }
}
