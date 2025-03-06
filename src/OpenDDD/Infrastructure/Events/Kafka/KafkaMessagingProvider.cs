using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using OpenDDD.Infrastructure.Events.Kafka.Factories;
using Confluent.Kafka;
using Confluent.Kafka.Admin;

namespace OpenDDD.Infrastructure.Events.Kafka
{
    public class KafkaMessagingProvider : IMessagingProvider, IAsyncDisposable
    {
        private readonly string _bootstrapServers;
        private readonly IProducer<Null, string> _producer;
        private readonly IAdminClient _adminClient;
        private readonly bool _autoCreateTopics;
        private readonly IKafkaConsumerFactory _consumerFactory;
        private readonly ILogger<KafkaMessagingProvider> _logger;
        private readonly ConcurrentDictionary<string, ConcurrentBag<KafkaConsumer>> _consumers = new();
        private readonly CancellationTokenSource _cts = new();
        private bool _disposed;

        public KafkaMessagingProvider(
            string bootstrapServers,
            IAdminClient adminClient,
            IProducer<Null, string> producer,
            IKafkaConsumerFactory consumerFactory,
            bool autoCreateTopics,
            ILogger<KafkaMessagingProvider> logger)
        {
            if (string.IsNullOrWhiteSpace(bootstrapServers))
                throw new ArgumentNullException(nameof(bootstrapServers));

            _bootstrapServers = bootstrapServers;
            _adminClient = adminClient ?? throw new ArgumentNullException(nameof(adminClient));
            _producer = producer ?? throw new ArgumentNullException(nameof(producer));
            _consumerFactory = consumerFactory ?? throw new ArgumentNullException(nameof(consumerFactory));
            _autoCreateTopics = autoCreateTopics;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        
        private static string GetGroupKey(string topic, string consumerGroup)
        {
            if (string.IsNullOrWhiteSpace(topic))
                throw new ArgumentException("Topic cannot be null or empty.", nameof(topic));

            if (string.IsNullOrWhiteSpace(consumerGroup))
                throw new ArgumentException("Consumer group cannot be null or empty.", nameof(consumerGroup));

            return $"{topic}:{consumerGroup}";
        }
        
        public async Task SubscribeAsync(
            string topic,
            string consumerGroup,
            Func<string, CancellationToken, Task> messageHandler,
            CancellationToken cancellationToken)
        {
            if (messageHandler is null)
                throw new ArgumentNullException(nameof(messageHandler));

            var groupKey = GetGroupKey(topic, consumerGroup);

            if (_autoCreateTopics)
            {
                await CreateTopicIfNotExistsAsync(topic, cancellationToken);
            }
            else
            {
                var metadata = _adminClient.GetMetadata(TimeSpan.FromSeconds(5));
                if (!metadata.Topics.Any(t => t.Topic == topic))
                {
                    _logger.LogError("Cannot subscribe to non-existent topic: {Topic}", topic);
                    throw new KafkaException(new Error(ErrorCode.UnknownTopicOrPart, $"Topic '{topic}' does not exist."));
                }
            }

            var consumers = _consumers.GetOrAdd(groupKey, _ => new ConcurrentBag<KafkaConsumer>());

            var newConsumer = _consumerFactory.Create(consumerGroup);
            newConsumer.Subscribe(topic);
            consumers.Add(newConsumer);

            _logger.LogDebug("Subscribed a new consumer to Kafka topic '{Topic}' with consumer group '{ConsumerGroup}'", topic, consumerGroup);

            newConsumer.StartProcessing(messageHandler, _cts.Token);
        }
        
        public async Task UnsubscribeAsync(string topic, string consumerGroup, CancellationToken cancellationToken)
        {
            var groupKey = GetGroupKey(topic, consumerGroup);

            if (!_consumers.TryGetValue(groupKey, out var consumers) || !consumers.Any())
            {
                _logger.LogWarning("No active consumers found for consumer group '{ConsumerGroup}' on topic '{Topic}'.", consumerGroup, topic);
                return;
            }

            _logger.LogDebug("Stopping all consumers for topic '{Topic}' and consumer group '{ConsumerGroup}'...", topic, consumerGroup);

            foreach (var consumer in consumers)
            {
                await consumer.StopProcessingAsync();
                consumer.Dispose();
            }

            _consumers.TryRemove(groupKey, out _);
        }

        public async Task PublishAsync(string topic, string message, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(topic))
                throw new ArgumentException("Topic cannot be null or empty.", nameof(topic));

            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Message cannot be null or empty.", nameof(message));

            if (_autoCreateTopics)
            {
                await CreateTopicIfNotExistsAsync(topic, cancellationToken);
            }

            await _producer.ProduceAsync(topic, new Message<Null, string> { Value = message }, cancellationToken);
            _logger.LogDebug("Published message to Kafka topic '{Topic}'", topic);
        }
        
        private async Task CreateTopicIfNotExistsAsync(string topic, CancellationToken cancellationToken)
        {
            try
            {
                var metadata = _adminClient.GetMetadata(TimeSpan.FromSeconds(5));

                if (metadata.Topics.Any(t => t.Topic == topic))
                {
                    _logger.LogDebug("Topic '{Topic}' already exists. Skipping creation.", topic);
                    return;
                }

                _logger.LogDebug("Creating Kafka topic: {Topic}", topic);
                await _adminClient.CreateTopicsAsync(new[]
                {
                    new TopicSpecification { Name = topic, NumPartitions = 2, ReplicationFactor = 1 }
                }, null);

                for (int i = 0; i < 30; i++)
                {
                    await Task.Delay(500, cancellationToken);
                    metadata = _adminClient.GetMetadata(TimeSpan.FromSeconds(1));

                    if (metadata.Topics.Any(t => t.Topic == topic))
                    {
                        _logger.LogDebug("Kafka topic '{Topic}' is now available.", topic);
                        return;
                    }
                }

                throw new KafkaException(new Error(ErrorCode.UnknownTopicOrPart, $"Failed to create topic '{topic}' within timeout."));
            }
            catch (KafkaException ex)
            {
                _logger.LogError(ex, "Kafka error while creating topic {Topic}: {Message}", topic, ex.Message);
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while creating Kafka topic {Topic}", topic);
                throw new InvalidOperationException($"Failed to create topic '{topic}'", ex);
            }
        }
        
        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;
            _disposed = true;

            _logger.LogDebug("Disposing KafkaMessagingProvider...");

            _cts.Cancel();

            var tasks = _consumers.Values
                .SelectMany(consumers => consumers)
                .Select(c => c.StopProcessingAsync())
                .ToList();

            await Task.WhenAll(tasks);

            foreach (var consumerList in _consumers.Values)
            {
                foreach (var consumer in consumerList)
                {
                    consumer.Dispose();
                }
            }

            _consumers.Clear();

            _producer.Dispose();
            _adminClient.Dispose();
        }
    }
}
