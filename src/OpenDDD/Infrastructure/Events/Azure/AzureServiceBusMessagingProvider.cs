using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using OpenDDD.Infrastructure.Events.Base;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;

namespace OpenDDD.Infrastructure.Events.Azure
{
    public class AzureServiceBusMessagingProvider : IMessagingProvider, IAsyncDisposable
    {
        private readonly ServiceBusClient _client;
        private readonly ServiceBusAdministrationClient _adminClient;
        private readonly bool _autoCreateTopics;
        private readonly ILogger<AzureServiceBusMessagingProvider> _logger;
        private readonly ConcurrentDictionary<string, AzureServiceBusSubscription> _subscriptions = new();
        private bool _disposed;

        public AzureServiceBusMessagingProvider(
            ServiceBusClient client,
            ServiceBusAdministrationClient adminClient,
            bool autoCreateTopics,
            ILogger<AzureServiceBusMessagingProvider> logger)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _adminClient = adminClient ?? throw new ArgumentNullException(nameof(adminClient));
            _autoCreateTopics = autoCreateTopics;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<ISubscription> SubscribeAsync(string topic, string consumerGroup, Func<string, CancellationToken, Task> messageHandler, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(topic))
            {
                throw new ArgumentException("Topic cannot be null or empty.", nameof(topic));
            }

            if (string.IsNullOrWhiteSpace(consumerGroup))
            {
                throw new ArgumentException("Consumer group cannot be null or empty.", nameof(consumerGroup));
            }

            if (messageHandler is null)
            {
                throw new ArgumentNullException(nameof(messageHandler));
            }

            var subscriptionName = consumerGroup;

            if (_autoCreateTopics)
            {
                await CreateTopicIfNotExistsAsync(topic, cancellationToken);
            }
            
            if (!await _adminClient.TopicExistsAsync(topic, cancellationToken))
            {
                var errorMessage = $"Cannot subscribe to topic '{topic}' because it does not exist and auto-creation is disabled.";
                _logger.LogError(errorMessage);
                throw new InvalidOperationException(errorMessage);
            }

            await CreateSubscriptionIfNotExistsAsync(topic, subscriptionName, cancellationToken);

            var processor = _client.CreateProcessor(topic, subscriptionName);

            processor.ProcessMessageAsync += async args =>
            {
                await messageHandler(args.Message.Body.ToString(), cancellationToken);
                await args.CompleteMessageAsync(args.Message, cancellationToken);
            };

            processor.ProcessErrorAsync += args =>
            {
                _logger.LogError(args.Exception, "Error processing message in subscription {SubscriptionName}", subscriptionName);
                return Task.CompletedTask;
            };

            var subscription = new AzureServiceBusSubscription(topic, consumerGroup, processor);
            _subscriptions[subscription.Id] = subscription;

            _logger.LogInformation("Starting message processor for topic '{Topic}' and subscription '{Subscription}', Subscription ID: {SubscriptionId}", topic, subscriptionName, subscription.Id);
            await processor.StartProcessingAsync(cancellationToken);

            return subscription;
        }

        public async Task UnsubscribeAsync(ISubscription subscription, CancellationToken cancellationToken = default)
        {
            if (subscription == null)
                throw new ArgumentNullException(nameof(subscription));

            if (subscription is not AzureServiceBusSubscription serviceBusSubscription || !_subscriptions.TryRemove(serviceBusSubscription.Id, out _))
            {
                _logger.LogWarning("No active subscription found with ID {SubscriptionId}", subscription.Id);
                return;
            }

            _logger.LogInformation("Unsubscribing from Azure Service Bus topic '{Topic}' and subscription '{Subscription}', Subscription ID: {SubscriptionId}", serviceBusSubscription.Topic, serviceBusSubscription.ConsumerGroup, serviceBusSubscription.Id);

            await serviceBusSubscription.Consumer.StopProcessingAsync(cancellationToken);
            await serviceBusSubscription.DisposeAsync();
        }

        public async Task PublishAsync(string topic, string message, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(topic))
            {
                throw new ArgumentException("Topic cannot be null or empty.", nameof(topic));
            }

            if (string.IsNullOrWhiteSpace(message))
            {
                throw new ArgumentException("Message cannot be null or empty.", nameof(message));
            }

            if (_autoCreateTopics)
            {
                await CreateTopicIfNotExistsAsync(topic, cancellationToken);
            }

            var sender = _client.CreateSender(topic);
            await sender.SendMessageAsync(new ServiceBusMessage(message), cancellationToken);
            _logger.LogInformation("Published message to topic '{Topic}'", topic);
        }

        private async Task CreateTopicIfNotExistsAsync(string topic, CancellationToken cancellationToken)
        {
            if (!await _adminClient.TopicExistsAsync(topic, cancellationToken))
            {
                await _adminClient.CreateTopicAsync(topic, cancellationToken);
                _logger.LogInformation("Created topic: {Topic}", topic);
            }
        }

        private async Task CreateSubscriptionIfNotExistsAsync(string topic, string subscriptionName, CancellationToken cancellationToken)
        {
            if (!await _adminClient.SubscriptionExistsAsync(topic, subscriptionName, cancellationToken))
            {
                await _adminClient.CreateSubscriptionAsync(topic, subscriptionName, cancellationToken);
                _logger.LogInformation("Created subscription: {Subscription} for topic: {Topic}", subscriptionName, topic);
            }
        }

        public async ValueTask DisposeAsync()
        {
            if (_disposed) return;
            _disposed = true;

            _logger.LogDebug("Disposing AzureServiceBusMessagingProvider...");

            foreach (var subscription in _subscriptions.Values)
            {
                if (subscription.Consumer.IsProcessing)
                {
                    await subscription.Consumer.StopProcessingAsync(CancellationToken.None);
                }
                await subscription.DisposeAsync();
            }

            _subscriptions.Clear();
            await _client.DisposeAsync();
        }
    }
}
