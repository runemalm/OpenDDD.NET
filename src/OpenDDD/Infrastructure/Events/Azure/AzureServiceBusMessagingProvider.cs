using Microsoft.Extensions.Logging;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;

namespace OpenDDD.Infrastructure.Events.Azure
{
    public class AzureServiceBusMessagingProvider : IMessagingProvider
    {
        private readonly ServiceBusClient _client;
        private readonly ServiceBusAdministrationClient _adminClient;
        private readonly bool _autoCreateTopics;
        private readonly ILogger<AzureServiceBusMessagingProvider> _logger;

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

        public async Task SubscribeAsync(string topic, string consumerGroup, Func<string, CancellationToken, Task> messageHandler, CancellationToken cancellationToken = default)
        {
            topic = topic.ToLower();
            var subscriptionName = consumerGroup;

            if (_autoCreateTopics)
            {
                await CreateTopicIfNotExistsAsync(topic, cancellationToken);
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

            _logger.LogInformation("Starting message processor for topic '{Topic}' and subscription '{Subscription}'", topic, subscriptionName);
            await processor.StartProcessingAsync(cancellationToken);
        }

        public async Task PublishAsync(string topic, string message, CancellationToken cancellationToken = default)
        {
            topic = topic.ToLower();

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
            topic = topic.ToLower();

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
    }
}
