using Microsoft.Extensions.Logging;
using Moq;
using OpenDDD.Infrastructure.Events.Azure;
using OpenDDD.Tests.Base;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;

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

        public AzureServiceBusMessagingProviderTests()
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
            // Cleanup any test topics/subscriptions from previous runs
            await CleanupTopicsAndSubscriptionsAsync();
        }

        public async Task DisposeAsync()
        {
            // We don't delete the topics after tests, in case we need to inspect them.
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
        public async Task Subscribe_WhenAutoCreateTopicsIsEnabled_ShouldCreateTopicIfNotExists()
        {
            // Arrange
            var topicName = $"test-topic-{Guid.NewGuid()}";

            // Ensure topic does not exist before the test
            if (await _adminClient.TopicExistsAsync(topicName))
            {
                await _adminClient.DeleteTopicAsync(topicName);
            }

            // Act
            await _messagingProvider.SubscribeAsync(topicName, "test-subscriber", (msg, token) => Task.CompletedTask);

            // Assert
            Assert.True(await _adminClient.TopicExistsAsync(topicName));
        }

        [Fact]
        public async Task Subscribe_WhenAutoCreateTopicsIsDisabled_ShouldNotCreateTopic_AndLogErrorWhenTopicNotExist()
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
    }
}
