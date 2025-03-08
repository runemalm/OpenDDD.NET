using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;
using OpenDDD.Infrastructure.Events.Azure;
using OpenDDD.Tests.Base;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using FluentAssertions;

namespace OpenDDD.Tests.Integration.Infrastructure.Events.Azure
{
    [Collection("AzureServiceBusTests")]
    public class AzureServiceBusMessagingProviderTests : IntegrationTests, IAsyncLifetime
    {
        private readonly string _connectionString;
        private readonly ServiceBusAdministrationClient _adminClient;
        private readonly ILogger<AzureServiceBusMessagingProvider> _logger;
        private readonly ServiceBusClient _serviceBusClient;
        private readonly AzureServiceBusMessagingProvider _messagingProvider;
        private readonly CancellationTokenSource _cts = new(TimeSpan.FromSeconds(120));

        public AzureServiceBusMessagingProviderTests(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper, enableLogging: true)
        {
            _connectionString = Environment.GetEnvironmentVariable("AZURE_SERVICE_BUS_CONNECTION_STRING")
                ?? throw new InvalidOperationException("AZURE_SERVICE_BUS_CONNECTION_STRING is not set.");

            _adminClient = new ServiceBusAdministrationClient(_connectionString);
            _serviceBusClient = new ServiceBusClient(_connectionString);
            _logger = LoggerFactory.CreateLogger<AzureServiceBusMessagingProvider>();
            
            _messagingProvider = new AzureServiceBusMessagingProvider(
                _serviceBusClient, 
                _adminClient, 
                autoCreateTopics: true, 
                _logger);
        }

        public async Task InitializeAsync()
        {
            await CleanupTopicsAndSubscriptionsAsync();
        }

        public async Task DisposeAsync()
        {
            await _cts.CancelAsync();
            await _messagingProvider.DisposeAsync();
        }

        private async Task CleanupTopicsAndSubscriptionsAsync()
        {
            var topics = _adminClient.GetTopicsAsync();
            await foreach (var topic in topics)
            {
                if (topic.Name.StartsWith("test-topic-"))
                {
                    var subscriptions = _adminClient.GetSubscriptionsAsync(topic.Name);
                    await foreach (var sub in subscriptions)
                    {
                        await _adminClient.DeleteSubscriptionAsync(topic.Name, sub.SubscriptionName);
                    }

                    await _adminClient.DeleteTopicAsync(topic.Name);
                }
            }
        }
        
        [Fact]
        public async Task AutoCreateTopic_ShouldCreateTopicOnSubscribe_WhenSettingEnabled()
        {
            // Arrange
            var topicName = $"test-topic-{Guid.NewGuid()}";
            var subscriptionName = "test-subscription";

            var topicExistsBefore = (await _adminClient.TopicExistsAsync(topicName)).Value;
            topicExistsBefore.Should().BeFalse("The topic should not exist before subscribing.");

            // Act
            await _messagingProvider.SubscribeAsync(topicName, subscriptionName, (msg, token) => Task.CompletedTask);

            // Assert
            Assert.True(await _adminClient.TopicExistsAsync(topicName));
        }
        
        [Fact]
        public async Task AutoCreateTopic_ShouldNotCreateTopicOnSubscribe_WhenSettingDisabled()
        {
            // Arrange
            var topicName = $"test-topic-{Guid.NewGuid()}";

            var topicExistsBefore = (await _adminClient.TopicExistsAsync(topicName)).Value;
            topicExistsBefore.Should().BeFalse("The topic should not exist before subscribing.");
        
            var messagingProvider = new AzureServiceBusMessagingProvider(
                _serviceBusClient, 
                _adminClient, 
                autoCreateTopics: false, 
                _logger);
        
            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await messagingProvider.SubscribeAsync(topicName, "test-subscriber", (msg, token) => Task.CompletedTask);
            });
            
            Assert.False(await _adminClient.TopicExistsAsync(topicName), "Topic should not have been created.");
        
            exception.Message.Should().Be($"Topic '{topicName}' does not exist. Enable 'autoCreateTopics' to create topics automatically.");
        }
        
        [Fact]
        public async Task AtLeastOnceGurantee_ShouldDeliverToLateSubscriber_WhenSubscribedBefore()
        {
            // Arrange
            var topicName = $"test-topic-{Guid.NewGuid()}";
            var subscriptionName = "test-subscription";
            var receivedMessages = new ConcurrentBag<string>();
            var messageToSend = "Persistent Message Test";
            var messageReceivedTcs = new TaskCompletionSource<bool>();

            var firstSubscription = await _messagingProvider.SubscribeAsync(topicName, subscriptionName, async (msg, token) =>
            {
                Assert.Fail("First subscription should not receive the message.");
            }, _cts.Token);
            
            await Task.Delay(500);

            await _messagingProvider.UnsubscribeAsync(firstSubscription, _cts.Token);
            
            await Task.Delay(500);

            // Act
            await _messagingProvider.PublishAsync(topicName, messageToSend, _cts.Token);

            await _messagingProvider.SubscribeAsync(topicName, subscriptionName, async (msg, token) =>
            {
                receivedMessages.Add(msg);
                messageReceivedTcs.TrySetResult(true);
            }, _cts.Token);

            // Wait for message with timeout
            await messageReceivedTcs.Task.WaitAsync(TimeSpan.FromSeconds(10));


            // Assert
            Assert.Contains(messageToSend, receivedMessages);
        }

        [Fact]
        public async Task AtLeastOnceGurantee_ShouldNotDeliverToLateSubscriber_WhenNotSubscribedBefore()
        {
            // Arrange
            var topicName = $"test-topic-{Guid.NewGuid()}";
            var subscriptionName = "test-subscription";
            var receivedMessages = new ConcurrentBag<string>();
            var messageToSend = "Non-Persistent Message Test";

            // Act
            await _messagingProvider.PublishAsync(topicName, messageToSend, _cts.Token);

            await Task.Delay(500);

            await _messagingProvider.SubscribeAsync(topicName, subscriptionName, async (msg, token) =>
            {
                receivedMessages.Add(msg);
            }, _cts.Token);

            await Task.Delay(5000);

            // Assert
            Assert.DoesNotContain(messageToSend, receivedMessages);
        }

        [Fact]
        public async Task AtLeastOnceGurantee_ShouldRedeliverLater_WhenMessageNotAcked()
        {
            // Arrange
            var topicName = $"test-topic-{Guid.NewGuid()}";
            var subscriptionName = "test-subscription";
            var receivedMessages = new ConcurrentBag<string>();
            var messageToSend = "Redelivery Test";

            async Task FaultyHandler(string msg, CancellationToken token)
            {
                receivedMessages.Add(msg);
                throw new Exception("Simulated consumer crash before acknowledgment.");
            }

            await _messagingProvider.SubscribeAsync(topicName, subscriptionName, FaultyHandler, _cts.Token);
            await Task.Delay(500);

            // Act
            await _messagingProvider.PublishAsync(topicName, messageToSend, _cts.Token);

            for (int i = 0; i < 300; i++)
            {
                if (receivedMessages.Count > 1) break;
                await Task.Delay(1000);
            }

            // Assert
            Assert.True(receivedMessages.Count > 1, "Message should be redelivered at least once.");
        }

        [Fact]
        public async Task CompetingConsumers_ShouldDeliverOnlyOnce_WhenMultipleConsumersInGroup()
        {
            // Arrange
            var topicName = $"test-topic-{Guid.NewGuid()}";
            var subscriptionName = "test-consumer-group";
            var receivedMessages = new ConcurrentDictionary<string, int>();
            var messageToSend = "Competing Consumer Test";

            async Task MessageHandler(string msg, CancellationToken token)
            {
                receivedMessages.AddOrUpdate("received", 1, (key, value) => value + 1);
            }

            await _messagingProvider.SubscribeAsync(topicName, subscriptionName, MessageHandler, _cts.Token);
            await _messagingProvider.SubscribeAsync(topicName, subscriptionName, MessageHandler, _cts.Token);
            await _messagingProvider.SubscribeAsync(topicName, subscriptionName, MessageHandler, _cts.Token);
            await Task.Delay(500);

            // Act
            await _messagingProvider.PublishAsync(topicName, messageToSend, _cts.Token);

            await Task.Delay(3000);

            // Assert
            Assert.Equal(1, receivedMessages.GetValueOrDefault("received", 0));
        }

        [Fact]
        public async Task CompetingConsumers_ShouldDistributeMessages_WhenMultipleConsumersInGroup()
        {
            // Arrange
            var topicName = $"test-topic-{Guid.NewGuid()}";
            var consumerGroup = "test-consumer-group";
            var totalMessages = 50;
            var numConsumers = 10;
            var variancePercentage = 0.1;
            var perConsumerMessageCount = new ConcurrentDictionary<Guid, int>(); // Track messages per consumer
            var allMessagesProcessed = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            async Task Subscribe()
            {
                var consumerId = Guid.NewGuid();

                async Task MessageHandler(string msg, CancellationToken token)
                {
                    perConsumerMessageCount.AddOrUpdate(consumerId, 1, (_, count) => count + 1);
                    _logger.LogDebug($"Subscriber {consumerId} received a message.");

                    if (perConsumerMessageCount.Values.Sum() >= totalMessages)
                    {
                        allMessagesProcessed.TrySetResult(true);
                    }
                }

                await _messagingProvider.SubscribeAsync(topicName, consumerGroup, MessageHandler, _cts.Token);
            }
            
            for (int i = 0; i < numConsumers; i++)
            {
                await Subscribe();
            }

            await Task.Delay(500);
            
            // Act
            for (int i = 0; i < totalMessages; i++)
            {
                await _messagingProvider.PublishAsync(topicName, "Test Message", _cts.Token);
            }

            try
            {
                await allMessagesProcessed.Task.WaitAsync(TimeSpan.FromSeconds(10));
            }
            catch (TimeoutException)
            {
                _logger.LogDebug("Timed out waiting for consumers to receive all messages.");
                Assert.Fail($"Consumers only processed {perConsumerMessageCount.Values.Sum()} of {totalMessages} messages.");
            }

            // Assert
            var messageCounts = perConsumerMessageCount.Values.ToList();
            var expectedPerConsumer = totalMessages / numConsumers;
            var variance = (int)(expectedPerConsumer * variancePercentage);
            var minAllowed = expectedPerConsumer - variance;
            var maxAllowed = expectedPerConsumer + variance;

            foreach (var count in messageCounts)
            {
                Assert.InRange(count, minAllowed, maxAllowed);
            }
        }
    }
}
