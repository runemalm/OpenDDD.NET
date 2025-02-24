﻿using System.Collections.Concurrent;
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
        private readonly ConcurrentDictionary<string, KafkaConsumer> _consumers = new();
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
        
        public async Task SubscribeAsync(
            string topic,
            string consumerGroup,
            Func<string, CancellationToken, Task> messageHandler,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(topic))
                throw new ArgumentException("Topic cannot be null or empty.", nameof(topic));

            if (string.IsNullOrWhiteSpace(consumerGroup))
                throw new ArgumentException("Consumer group cannot be null or empty.", nameof(consumerGroup));

            if (messageHandler is null)
                throw new ArgumentNullException(nameof(messageHandler));
            
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

            var kafkaConsumer = _consumers.GetOrAdd(consumerGroup, _ => _consumerFactory.Create(consumerGroup));

            kafkaConsumer.Subscribe(topic);

            _logger.LogDebug("Subscribed to Kafka topic '{Topic}' with consumer group '{ConsumerGroup}'", topic, consumerGroup);

            kafkaConsumer.StartProcessing(messageHandler, _cts.Token);
        }
        
        public async Task UnsubscribeAsync(string topic, string consumerGroup, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(topic))
                throw new ArgumentException("Topic cannot be null or empty.", nameof(topic));

            if (string.IsNullOrWhiteSpace(consumerGroup))
                throw new ArgumentException("Consumer group cannot be null or empty.", nameof(consumerGroup));
            
            if (!_consumers.TryRemove(consumerGroup, out var kafkaConsumer))
            {
                _logger.LogWarning("No active consumer found for consumer group '{ConsumerGroup}'. It may have already stopped.", consumerGroup);
                return;
            }

            _logger.LogInformation("Stopping consumer group '{ConsumerGroup}'...", consumerGroup);
            await kafkaConsumer.StopProcessingAsync();
            kafkaConsumer.Dispose();
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
                    new TopicSpecification { Name = topic, NumPartitions = 1, ReplicationFactor = 1 }
                }, null);

                for (int i = 0; i < 10; i++)
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
            
            var tasks = _consumers.Values.Select(c => c.StopProcessingAsync()).ToList();
            await Task.WhenAll(tasks);

            foreach (var consumer in _consumers.Values)
            {
                consumer.Dispose();
            }

            _producer.Dispose();
            _adminClient.Dispose();
        }
    }
}
