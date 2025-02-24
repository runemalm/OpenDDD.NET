using Confluent.Kafka;
using Microsoft.Extensions.Logging;

namespace OpenDDD.Infrastructure.Events.Kafka.Factories
{
    public class KafkaConsumerFactory : IKafkaConsumerFactory
    {
        private readonly string _bootstrapServers;
        private readonly ILogger<KafkaConsumer> _logger;

        public KafkaConsumerFactory(string bootstrapServers, ILogger<KafkaConsumer> logger)
        {
            if (string.IsNullOrWhiteSpace(bootstrapServers))
                throw new ArgumentException("Kafka bootstrap servers must be configured.", nameof(bootstrapServers));

            _bootstrapServers = bootstrapServers;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public virtual KafkaConsumer Create(string consumerGroup)
        {
            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = _bootstrapServers,
                ClientId = "OpenDDD",
                GroupId = consumerGroup,
                EnableAutoCommit = false,
                AutoOffsetReset = AutoOffsetReset.Latest,
                MaxPollIntervalMs = 300000, // Max time consumer can take to process a message before kafka removes it from group
                SessionTimeoutMs = 45000, // Time before kafka assumes consumer is dead if it stops sending heartbeats
                HeartbeatIntervalMs = 3000 // Frequence of heartbeats to kafka
            };

            var consumer = new ConsumerBuilder<Ignore, string>(consumerConfig)
                .SetValueDeserializer(Deserializers.Utf8)
                .Build();
            return new KafkaConsumer(consumer, consumerGroup, _logger);
        }
    }
}
