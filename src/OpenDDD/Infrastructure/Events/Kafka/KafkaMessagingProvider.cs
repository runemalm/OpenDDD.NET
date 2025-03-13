﻿using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using OpenDDD.Infrastructure.Events.Base;
using OpenDDD.Infrastructure.Events.Kafka.Factories;
using Confluent.Kafka;
using Confluent.Kafka.Admin;

namespace OpenDDD.Infrastructure.Events.Kafka
{
    public class KafkaMessagingProvider : IMessagingProvider, IAsyncDisposable
    {
        private readonly IProducer<Null, string> _producer;
        private readonly IAdminClient _adminClient;
        private readonly bool _autoCreateTopics;
        private readonly IKafkaConsumerFactory _consumerFactory;
        private readonly ILogger<KafkaMessagingProvider> _logger;
        private readonly ConcurrentDictionary<string, KafkaSubscription> _subscriptions = new();
        private readonly ConcurrentDictionary<string, DateTime> _topicCache = new();
        private readonly TimeSpan _cacheExpiration = TimeSpan.FromSeconds(600);
        private readonly CancellationTokenSource _cts = new();
        private bool _disposed;

        public KafkaMessagingProvider(
            IAdminClient adminClient,
            IProducer<Null, string> producer,
            IKafkaConsumerFactory consumerFactory,
            bool autoCreateTopics,
            ILogger<KafkaMessagingProvider> logger)
        {
            _adminClient = adminClient ?? throw new ArgumentNullException(nameof(adminClient));
            _producer = producer ?? throw new ArgumentNullException(nameof(producer));
            _consumerFactory = consumerFactory ?? throw new ArgumentNullException(nameof(consumerFactory));
            _autoCreateTopics = autoCreateTopics;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ISubscription> SubscribeAsync(
            string topic,
            string consumerGroup,
            Func<string, CancellationToken, Task> messageHandler,
            CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(topic))
                throw new ArgumentException("Topic cannot be null or empty.", nameof(topic));

            if (string.IsNullOrWhiteSpace(consumerGroup))
                throw new ArgumentException("Consumer group cannot be null or empty.", nameof(consumerGroup));

            if (messageHandler is null)
                throw new ArgumentNullException(nameof(messageHandler));

            await EnsureTopicExistsAsync(topic, cancellationToken);

            var consumer = _consumerFactory.Create(consumerGroup);
            consumer.Subscribe(topic);
            consumer.StartProcessing(messageHandler, _cts.Token);

            var subscription = new KafkaSubscription(topic, consumerGroup, consumer);
            _subscriptions[subscription.Id] = subscription;

            _logger.LogDebug("Subscribed a new consumer to Kafka topic '{Topic}' with consumer group '{ConsumerGroup}', Subscription ID: {SubscriptionId}", topic, consumerGroup, subscription.Id);
            return subscription;
        }

        public async Task UnsubscribeAsync(ISubscription subscription, CancellationToken cancellationToken = default)
        {
            if (subscription == null)
                throw new ArgumentNullException(nameof(subscription));

            if (!_subscriptions.TryRemove(subscription.Id, out var removedSubscription))
            {
                _logger.LogWarning("No active subscription found with ID {SubscriptionId}", subscription.Id);
                return;
            }

            _logger.LogDebug("Unsubscribing from Kafka topic '{Topic}' with consumer group '{ConsumerGroup}', Subscription ID: {SubscriptionId}", removedSubscription.Topic, removedSubscription.ConsumerGroup, removedSubscription.Id);

            await removedSubscription.Consumer.StopProcessingAsync();
            await removedSubscription.DisposeAsync();
        }

        public async Task PublishAsync(string topic, string message, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(topic))
                throw new ArgumentException("Topic cannot be null or empty.", nameof(topic));

            if (string.IsNullOrWhiteSpace(message))
                throw new ArgumentException("Message cannot be null or empty.", nameof(message));

            await EnsureTopicExistsAsync(topic, cancellationToken);

            await _producer.ProduceAsync(topic, new Message<Null, string> { Value = message }, cancellationToken);
            _logger.LogDebug("Published message to Kafka topic '{Topic}'", topic);
        }

        private async Task EnsureTopicExistsAsync(string topic, CancellationToken cancellationToken)
        {
            if (_topicCache.TryGetValue(topic, out var lastChecked) && DateTime.UtcNow - lastChecked < _cacheExpiration)
            {
                _logger.LogDebug("Skipping topic check for '{Topic}' (cached result).", topic);
                return;
            }

            var metadata = _adminClient.GetMetadata(TimeSpan.FromSeconds(5));

            if (metadata.Topics.Any(t => t.Topic == topic))
            {
                _logger.LogDebug("Topic '{Topic}' already exists.", topic);
                _topicCache[topic] = DateTime.UtcNow;
                return;
            }

            if (_autoCreateTopics)
            {
                _logger.LogDebug("Creating Kafka topic: {Topic}", topic);
                await _adminClient.CreateTopicsAsync(new[]
                {
                    new TopicSpecification { Name = topic, NumPartitions = 2, ReplicationFactor = 1 }
                }, null);

                _topicCache[topic] = DateTime.UtcNow;
                
                // Wait for the topic to be available
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

            throw new InvalidOperationException($"Topic '{topic}' does not exist. Enable 'autoCreateTopics' to create topics automatically.");
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;
            _disposed = true;

            _logger.LogDebug("Disposing KafkaMessagingProvider...");

            _cts.Cancel();

            var disposeTasks = _subscriptions.Values.Select(async sub =>
            {
                await sub.Consumer.StopProcessingAsync();
                await sub.DisposeAsync();
            }).ToList();

            await Task.WhenAll(disposeTasks);

            _subscriptions.Clear();
            _producer.Dispose();
            _adminClient.Dispose();
        }
    }
}
