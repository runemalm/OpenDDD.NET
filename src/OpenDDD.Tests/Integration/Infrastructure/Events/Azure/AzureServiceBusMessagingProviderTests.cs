using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Moq;
using OpenDDD.Infrastructure.Events.Azure;
using OpenDDD.Tests.Base;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Xunit.Abstractions;

namespace OpenDDD.Tests.Integration.Infrastructure.Events.Azure
{
    [Collection("AzureServiceBusTests")] // Ensure tests run sequentially
    public class AzureServiceBusMessagingProviderTests : IntegrationTests, IAsyncLifetime
    {
        private readonly string _connectionString;
        private readonly ServiceBusAdministrationClient _adminClient;
        private readonly Mock<ILogger<AzureServiceBusMessagingProvider>> _loggerMock;
        private readonly ServiceBusClient _serviceBusClient;
        private readonly AzureServiceBusMessagingProvider _messagingProvider;

        public AzureServiceBusMessagingProviderTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _connectionString = Environment.GetEnvironmentVariable("AZURE_SERVICE_BUS_CONNECTION_STRING")
                ?? throw new InvalidOperationException("AZURE_SERVICE_BUS_CONNECTION_STRING is not set.");

            _adminClient = new ServiceBusAdministrationClient(_connectionString);
            _serviceBusClient = new ServiceBusClient(_connectionString);
            _loggerMock = new Mock<ILogger<AzureServiceBusMessagingProvider>>();

            _messagingProvider = new AzureServiceBusMessagingProvider(
                _serviceBusClient, 
                _adminClient, 
                autoCreateTopics: true, 
                _loggerMock.Object);
        }

        public async Task InitializeAsync()
        {
            await CleanupTopicsAndSubscriptionsAsync();
        }

        public async Task DisposeAsync()
        {
            
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

            if (await _adminClient.TopicExistsAsync(topicName))
            {
                await _adminClient.DeleteTopicAsync(topicName);
            }

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
        
            // Ensure topic does not exist before the test
            if (await _adminClient.TopicExistsAsync(topicName))
            {
                await _adminClient.DeleteTopicAsync(topicName);
            }
        
            var messagingProvider = new AzureServiceBusMessagingProvider(
                _serviceBusClient, 
                _adminClient, 
                autoCreateTopics: false, 
                _loggerMock.Object);
        
            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await messagingProvider.SubscribeAsync(topicName, "test-subscriber", (msg, token) => Task.CompletedTask);
            });
            
            Assert.False(await _adminClient.TopicExistsAsync(topicName), "Topic should not have been created.");
        
            Assert.Equal($"Cannot subscribe to topic '{topicName}' because it does not exist and auto-creation is disabled.", exception.Message);
        }
        
        [Fact]
        public async Task AtLeastOnceGurantee_ShouldDeliverToLateSubscriber_WhenSubscribedBefore()
        {
            // Arrange
            var topicName = $"test-topic-{Guid.NewGuid()}";
            var subscriptionName = "test-subscription";
            var receivedMessages = new ConcurrentBag<string>();
            var messageToSend = "Persistent Message Test";
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            await _messagingProvider.SubscribeAsync(topicName, subscriptionName, async (msg, token) =>
            {
                Assert.Fail("First subscription should not receive the message.");
            }, cts.Token);
            await Task.Delay(500);

            await _messagingProvider.UnsubscribeAsync(topicName, subscriptionName, cts.Token);
            await Task.Delay(500);

            // Act: Publish message
            await _messagingProvider.PublishAsync(topicName, messageToSend, cts.Token);

            // Delay to simulate late subscriber
            await Task.Delay(2000);

            // Late subscriber
            await _messagingProvider.SubscribeAsync(topicName, subscriptionName, async (msg, token) =>
            {
                receivedMessages.Add(msg);
            }, cts.Token);

            // Wait for message processing
            await Task.Delay(1000);

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
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            // Act: Publish message before any subscription
            await _messagingProvider.PublishAsync(topicName, messageToSend, cts.Token);

            await Task.Delay(500);

            // Late subscriber
            await _messagingProvider.SubscribeAsync(topicName, subscriptionName, async (msg, token) =>
            {
                receivedMessages.Add(msg);
            }, cts.Token);

            // Wait for message processing
            await Task.Delay(1000);

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
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            async Task FaultyHandler(string msg, CancellationToken token)
            {
                receivedMessages.Add(msg);
                throw new Exception("Simulated consumer crash before acknowledgment.");
            }

            await _messagingProvider.SubscribeAsync(topicName, subscriptionName, FaultyHandler, cts.Token);
            await Task.Delay(500);

            // Act: Publish message
            await _messagingProvider.PublishAsync(topicName, messageToSend, cts.Token);

            // Wait for redelivery
            await Task.Delay(3000);

            // Assert: The message should be received multiple times due to reattempts
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
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            async Task MessageHandler(string msg, CancellationToken token)
            {
                receivedMessages.AddOrUpdate("received", 1, (key, value) => value + 1);
            }

            // Multiple competing consumers in the same group
            await _messagingProvider.SubscribeAsync(topicName, subscriptionName, MessageHandler, cts.Token);
            await _messagingProvider.SubscribeAsync(topicName, subscriptionName, MessageHandler, cts.Token);
            await _messagingProvider.SubscribeAsync(topicName, subscriptionName, MessageHandler, cts.Token);
            await Task.Delay(500);

            // Act
            await _messagingProvider.PublishAsync(topicName, messageToSend, cts.Token);

            // Wait for processing
            await Task.Delay(1000);

            // Assert: Only one of the competing consumers should receive the message
            Assert.Equal(1, receivedMessages.GetValueOrDefault("received", 0));
        }

        [Fact]
        public async Task CompetingConsumers_ShouldDistributeEvenly_WhenMultipleConsumersInGroup()
        {
            // Arrange
            var topicName = $"test-topic-{Guid.NewGuid()}";
            var subscriptionName = "test-consumer-group";
            var receivedMessages = new ConcurrentDictionary<string, int>();
            var totalMessages = 10;
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            async Task MessageHandler(string msg, CancellationToken token)
            {
                receivedMessages.AddOrUpdate(msg, 1, (key, value) => value + 1);
            }

            // Multiple competing consumers in the same group
            await _messagingProvider.SubscribeAsync(topicName, subscriptionName, MessageHandler, cts.Token);
            await _messagingProvider.SubscribeAsync(topicName, subscriptionName, MessageHandler, cts.Token);
            await _messagingProvider.SubscribeAsync(topicName, subscriptionName, MessageHandler, cts.Token);
            await Task.Delay(500);

            // Act: Publish multiple messages
            for (int i = 0; i < totalMessages; i++)
            {
                await _messagingProvider.PublishAsync(topicName, $"Message {i}", cts.Token);
            }

            // Wait for processing
            await Task.Delay(2000);

            // Assert: Messages should be evenly distributed across consumers
            var messageCounts = receivedMessages.Values;
            var minReceived = messageCounts.Min();
            var maxReceived = messageCounts.Max();

            Assert.True(maxReceived - minReceived <= 1,
                "Messages should be evenly distributed among competing consumers.");
        }
    }
}
