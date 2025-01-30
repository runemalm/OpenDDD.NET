using OpenDDD.Infrastructure.Events.Azure.Options;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;

namespace OpenDDD.Infrastructure.Events.Azure
{
    public class AzureServiceBusProvider : IMessagingProvider
    {
        private readonly ServiceBusClient _client;
        private readonly AzureServiceBusOptions _options;

        public AzureServiceBusProvider(AzureServiceBusOptions options)
        {
            _options = options ?? throw new ArgumentNullException(nameof(options));
            _client = new ServiceBusClient(options.ConnectionString);
        }

        public async Task SubscribeAsync(string topic, string consumerGroup, Func<string, CancellationToken, Task> messageHandler, CancellationToken cancellationToken = default)
        {
            var subscriptionName = consumerGroup;

            if (_options.AutoCreateTopics)
            {
                await CreateSubscriptionIfNotExistsAsync(topic, subscriptionName, cancellationToken);
            }

            var processor = _client.CreateProcessor(topic, subscriptionName);

            processor.ProcessMessageAsync += async args =>
            {
                await messageHandler(args.Message.Body.ToString(), cancellationToken);
                await args.CompleteMessageAsync(args.Message, cancellationToken);
            };

            processor.ProcessErrorAsync += args =>
            {
                Console.WriteLine($"Error processing message in subscription {subscriptionName}: {args.Exception.Message}");
                return Task.CompletedTask;
            };

            await processor.StartProcessingAsync(cancellationToken);
        }

        public async Task PublishAsync(string topic, string message, CancellationToken cancellationToken = default)
        {
            if (_options.AutoCreateTopics)
            {
                await CreateTopicIfNotExistsAsync(topic, cancellationToken);
            }

            var sender = _client.CreateSender(topic);
            await sender.SendMessageAsync(new ServiceBusMessage(message), cancellationToken);
        }

        private async Task CreateTopicIfNotExistsAsync(string topic, CancellationToken cancellationToken)
        {
            var adminClient = new ServiceBusAdministrationClient(_options.ConnectionString);

            if (!await adminClient.TopicExistsAsync(topic, cancellationToken))
            {
                await adminClient.CreateTopicAsync(topic, cancellationToken);
                Console.WriteLine($"Created topic: {topic}");
            }
        }

        private async Task CreateSubscriptionIfNotExistsAsync(string topic, string subscriptionName, CancellationToken cancellationToken)
        {
            var adminClient = new ServiceBusAdministrationClient(_options.ConnectionString);

            if (!await adminClient.SubscriptionExistsAsync(topic, subscriptionName, cancellationToken))
            {
                await adminClient.CreateSubscriptionAsync(topic, subscriptionName, cancellationToken);
                Console.WriteLine($"Created subscription: {subscriptionName} for topic: {topic}");
            }
        }
    }
}
