using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using OpenDDD.Infrastructure.Events.Azure.Options;
using OpenDDD.Main.Options;

namespace OpenDDD.Infrastructure.Events.Azure
{
    public class AzureServiceBusProvider : IMessagingProvider
    {
        private readonly ServiceBusClient _client;
        private readonly AzureServiceBusOptions _options;
        private readonly OpenDddOptions _openDddOptions;

        public AzureServiceBusProvider(AzureServiceBusOptions options, OpenDddOptions openDddOptions)
        {
            _openDddOptions = openDddOptions;
            _options = options;
            _client = new ServiceBusClient(options.ConnectionString);
        }

        public async Task SubscribeAsync(string topic, Func<string, Task> messageHandler)
        {
            var subscriptionName = _openDddOptions.EventsListenerGroup;

            if (_options.AutoCreateTopics)
            {
                await CreateSubscriptionIfNotExistsAsync(topic, subscriptionName);
            }

            var processor = _client.CreateProcessor(topic, subscriptionName);
            processor.ProcessMessageAsync += async args =>
            {
                await messageHandler(args.Message.Body.ToString());
                await args.CompleteMessageAsync(args.Message);
            };
            processor.ProcessErrorAsync += args =>
            {
                Console.WriteLine($"Error processing message in subscription {subscriptionName}: {args.Exception.Message}");
                return Task.CompletedTask;
            };

            await processor.StartProcessingAsync();
        }

        public async Task PublishAsync(string topic, string message)
        {
            if (_options.AutoCreateTopics)
            {
                await CreateTopicIfNotExistsAsync(topic);
            }

            var sender = _client.CreateSender(topic);
            await sender.SendMessageAsync(new ServiceBusMessage(message));
        }

        private async Task CreateTopicIfNotExistsAsync(string topic)
        {
            var adminClient = new ServiceBusAdministrationClient(_options.ConnectionString);

            if (!await adminClient.TopicExistsAsync(topic))
            {
                await adminClient.CreateTopicAsync(topic);
                Console.WriteLine($"Created topic: {topic}");
            }
        }
        
        private async Task CreateSubscriptionIfNotExistsAsync(string topic, string subscriptionName)
        {
            var adminClient = new ServiceBusAdministrationClient(_options.ConnectionString);

            if (!await adminClient.SubscriptionExistsAsync(topic, subscriptionName))
            {
                await adminClient.CreateSubscriptionAsync(topic, subscriptionName);
                Console.WriteLine($"Created subscription: {subscriptionName} for topic: {topic}");
            }
        }
    }
}