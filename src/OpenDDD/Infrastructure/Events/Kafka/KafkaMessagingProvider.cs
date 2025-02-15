using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenDDD.API.Options;
using OpenDDD.Infrastructure.Events.Kafka.Options;
using Confluent.Kafka;
using Confluent.Kafka.Admin;

namespace OpenDDD.Infrastructure.Events.Kafka
{
    public class KafkaMessagingProvider : IMessagingProvider, IAsyncDisposable
    {
        private readonly IProducer<Null, string> _producer;
        private readonly IAdminClient _adminClient;
        private readonly OpenDddKafkaOptions _options;
        private readonly ILogger<KafkaMessagingProvider> _logger;

        public KafkaMessagingProvider(IOptions<OpenDddOptions> options, ILogger<KafkaMessagingProvider> logger)
        {
            var openDddOptions = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _options = openDddOptions.Kafka ?? throw new InvalidOperationException("Kafka settings are missing in OpenDddOptions.");

            if (string.IsNullOrWhiteSpace(_options.BootstrapServers))
                throw new InvalidOperationException("Kafka bootstrap servers must be configured.");

            var producerConfig = new ProducerConfig
            {
                BootstrapServers = _options.BootstrapServers,
                ClientId = "OpenDDD"
            };

            var adminConfig = new AdminClientConfig
            {
                BootstrapServers = _options.BootstrapServers,
                ClientId = "OpenDDD"
            };

            _producer = new ProducerBuilder<Null, string>(producerConfig).Build();
            _adminClient = new AdminClientBuilder(adminConfig).Build();

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private async Task EnsureTopicExistsAsync(string topic, CancellationToken cancellationToken)
        {
            try
            {
                var metadata = _adminClient.GetMetadata(topic, TimeSpan.FromSeconds(5));
                if (metadata.Topics.Any(t => t.Topic == topic)) return; // Topic already exists

                _logger.LogInformation("Creating Kafka topic: {Topic}", topic);
                await _adminClient.CreateTopicsAsync(new[]
                {
                    new TopicSpecification { Name = topic, NumPartitions = 1, ReplicationFactor = 1 }
                });
            }
            catch (Exception ex)
            {
                _logger.LogWarning("Could not check or create Kafka topic {Topic}: {Message}", topic, ex.Message);
            }
        }

        public async Task PublishAsync(string topic, string message, CancellationToken cancellationToken = default)
        {
            await EnsureTopicExistsAsync(topic, cancellationToken);

            await _producer.ProduceAsync(topic, new Message<Null, string> { Value = message }, cancellationToken);
            _logger.LogInformation("Published message to Kafka topic '{Topic}'", topic);
        }

        public async Task SubscribeAsync(string topic, string consumerGroup, Func<string, CancellationToken, Task> messageHandler, CancellationToken cancellationToken = default)
        {
            await EnsureTopicExistsAsync(topic, cancellationToken);

            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = _options.BootstrapServers,
                ClientId = "OpenDDD",
                GroupId = consumerGroup,
                EnableAutoCommit = false,
                AutoOffsetReset = AutoOffsetReset.Earliest
            };

            var consumer = new ConsumerBuilder<Ignore, string>(consumerConfig)
                .SetValueDeserializer(Deserializers.Utf8)
                .Build();

            consumer.Subscribe(topic);

            _logger.LogInformation("Subscribed to Kafka topic '{Topic}' with consumer group '{ConsumerGroup}'", topic, consumerGroup);

            _ = Task.Run(async () =>
            {
                try
                {
                    while (!cancellationToken.IsCancellationRequested)
                    {
                        // Seems like consume don't respect cancellation token.
                        // See: https://github.com/confluentinc/confluent-kafka-dotnet/issues/1085
                        var result = consumer.Consume(cancellationToken);
                        if (result.Message != null)
                        {
                            _logger.LogInformation("Received message from Kafka: {Message}", result.Message.Value);
                            await messageHandler(result.Message.Value, cancellationToken);
                            consumer.Commit(result);
                            _logger.LogInformation("Message processed and offset committed: {Offset}", result.Offset);
                        }
                    }
                }
                catch (OperationCanceledException)
                {
                    // Handle cancellation gracefully
                }
                finally
                {
                    consumer.Close();
                }
            }, cancellationToken);
        }

        public async ValueTask DisposeAsync()
        {
            _producer?.Dispose();
            _adminClient?.Dispose();
        }
    }
}
