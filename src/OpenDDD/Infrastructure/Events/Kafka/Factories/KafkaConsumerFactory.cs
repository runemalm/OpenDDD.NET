using Confluent.Kafka;

namespace OpenDDD.Infrastructure.Events.Kafka.Factories
{
    public class KafkaConsumerFactory
    {
        private readonly string _bootstrapServers;

        public KafkaConsumerFactory(string bootstrapServers)
        {
            if (string.IsNullOrWhiteSpace(bootstrapServers))
                throw new ArgumentException("Kafka bootstrap servers must be configured.", nameof(bootstrapServers));

            _bootstrapServers = bootstrapServers;
        }

        public virtual IConsumer<Ignore, string> Create(string consumerGroup)
        {
            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = _bootstrapServers,
                ClientId = "OpenDDD",
                GroupId = consumerGroup,
                EnableAutoCommit = false,
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            return new ConsumerBuilder<Ignore, string>(consumerConfig)
                .SetValueDeserializer(Deserializers.Utf8)
                .Build();
        }
    }
}
