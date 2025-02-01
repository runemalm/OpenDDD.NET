using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenDDD.API.Options;
using OpenDDD.Infrastructure.Events.Azure.Options;

namespace OpenDDD.Infrastructure.Events.Azure
{
    public class AzureServiceBusMessagingProvider : IMessagingProvider
    {
        private readonly ServiceBusClient _client;
        private readonly OpenDddAzureServiceBusOptions _options;
        private readonly ILogger<AzureServiceBusMessagingProvider> _logger;

        public AzureServiceBusMessagingProvider(
            IOptions<OpenDddOptions> options,
            ILogger<AzureServiceBusMessagingProvider> logger)
        {
            var openDddOptions = options.Value ?? throw new ArgumentNullException(nameof(options));
            _options = openDddOptions.AzureServiceBus ?? throw new InvalidOperationException("AzureServiceBus settings are missing in OpenDddOptions.");
        
            if (string.IsNullOrWhiteSpace(_options.ConnectionString))
            {
                throw new InvalidOperationException("Azure Service Bus connection string is missing.");
            }

            _client = new ServiceBusClient(_options.ConnectionString);
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task SubscribeAsync(string topic, string consumerGroup, Func<string, CancellationToken, Task> messageHandler, CancellationToken cancellationToken = default)
        {
            topic = topic.ToLower();

            var subscriptionName = consumerGroup;

            if (_options.AutoCreateTopics)
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

            if (_options.AutoCreateTopics)
            {
                await CreateTopicIfNotExistsAsync(topic, cancellationToken);
            }

            var sender = _client.CreateSender(topic);
            await sender.SendMessageAsync(new ServiceBusMessage(message), cancellationToken);
            _logger.LogInformation("Published message to topic '{Topic}'", topic);
        }

        private async Task CreateTopicIfNotExistsAsync(string topic, CancellationToken cancellationToken)
        {
            var adminClient = new ServiceBusAdministrationClient(_options.ConnectionString);
            topic = topic.ToLower();

            if (!await adminClient.TopicExistsAsync(topic, cancellationToken))
            {
                await adminClient.CreateTopicAsync(topic, cancellationToken);
                _logger.LogInformation("Created topic: {Topic}", topic);
            }
        }

        private async Task CreateSubscriptionIfNotExistsAsync(string topic, string subscriptionName, CancellationToken cancellationToken)
        {
            var adminClient = new ServiceBusAdministrationClient(_options.ConnectionString);

            if (!await adminClient.SubscriptionExistsAsync(topic, subscriptionName, cancellationToken))
            {
                await adminClient.CreateSubscriptionAsync(topic, subscriptionName, cancellationToken);
                _logger.LogInformation("Created subscription: {Subscription} for topic: {Topic}", subscriptionName, topic);
            }
        }
    }
}
