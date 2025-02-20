using System.Text;
using Microsoft.Extensions.Logging;
using Moq;
using Azure;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using OpenDDD.Infrastructure.Events.Azure;
using OpenDDD.Tests.Base;

namespace OpenDDD.Tests.Infrastructure.Events.Azure
{
    public class AzureServiceBusMessagingProviderTests : UnitTests
    {
        private readonly Mock<ServiceBusClient> _mockClient;
        private readonly Mock<ServiceBusAdministrationClient> _mockAdminClient;
        private readonly Mock<ServiceBusSender> _mockSender;
        private readonly Mock<ServiceBusProcessor> _mockProcessor;
        private readonly Mock<ILogger<AzureServiceBusMessagingProvider>> _mockLogger;
        private readonly AzureServiceBusMessagingProvider _provider;
        private readonly string _testTopic = "test-topic";
        private readonly string _testSubscription = "test-subscription";

        public AzureServiceBusMessagingProviderTests()
        {
            _mockClient = new Mock<ServiceBusClient>();
            _mockAdminClient = new Mock<ServiceBusAdministrationClient>();
            _mockSender = new Mock<ServiceBusSender>();
            _mockProcessor = new Mock<ServiceBusProcessor>();
            _mockLogger = new Mock<ILogger<AzureServiceBusMessagingProvider>>();

            _mockClient
                .Setup(client => client.CreateSender(It.IsAny<string>()))
                .Returns(_mockSender.Object);

            _mockClient
                .Setup(client => client.CreateProcessor(It.IsAny<string>(), It.IsAny<string>()))
                .Returns(_mockProcessor.Object);

            _mockAdminClient
                .Setup(admin => admin.TopicExistsAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Response.FromValue(true, Mock.Of<Response>()));

            _mockAdminClient
                .Setup(admin => admin.SubscriptionExistsAsync(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(Response.FromValue(true, Mock.Of<Response>()));

            _provider = new AzureServiceBusMessagingProvider(
                client: _mockClient.Object,
                adminClient: _mockAdminClient.Object,
                autoCreateTopics: true,
                logger: _mockLogger.Object
            );
        }
        
        [Theory]
        [InlineData(null, "adminClient", "logger")]
        [InlineData("client", null, "logger")]
        [InlineData("client", "adminClient", null)]
        public void Constructor_ShouldThrowException_WhenDependenciesAreNull(
            string? client, string? adminClient, string? logger)
        {
            var mockClient = client is null ? null! : _mockClient.Object;
            var mockAdminClient = adminClient is null ? null! : _mockAdminClient.Object;
            var mockLogger = logger is null ? null! : _mockLogger.Object;
        
            Assert.Throws<ArgumentNullException>(() =>
                new AzureServiceBusMessagingProvider(mockClient, mockAdminClient, true, mockLogger));
        }
        
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task SubscribeAsync_ShouldThrowException_WhenTopicIsInvalid(string invalidTopic)
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _provider.SubscribeAsync(invalidTopic, _testSubscription, (msg, token) => Task.CompletedTask, CancellationToken.None));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task SubscribeAsync_ShouldThrowException_WhenConsumerGroupIsInvalid(string invalidConsumerGroup)
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _provider.SubscribeAsync(_testTopic, invalidConsumerGroup, (msg, token) => Task.CompletedTask, CancellationToken.None));
        }

        [Fact]
        public async Task SubscribeAsync_ShouldThrowException_WhenHandlerIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _provider.SubscribeAsync(_testTopic, _testSubscription, null!, CancellationToken.None));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task PublishAsync_ShouldThrowException_WhenTopicIsInvalid(string invalidTopic)
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _provider.PublishAsync(invalidTopic, "message", CancellationToken.None));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task PublishAsync_ShouldThrowException_WhenMessageIsInvalid(string invalidMessage)
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _provider.PublishAsync(_testTopic, invalidMessage, CancellationToken.None));
        }
        
        [Fact]
        public async Task SubscribeAsync_ShouldCreateTopicIfNotExists_WhenAutoCreateEnabled()
        {
            // Arrange: topic don't exist
            _mockAdminClient.Setup(admin => admin.TopicExistsAsync(_testTopic, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Response.FromValue(false, Mock.Of<Response>()));

            // Act
            await _provider.SubscribeAsync(_testTopic, _testSubscription, (msg, token) => Task.CompletedTask, CancellationToken.None);

            // Assert: create-topic was called
            _mockAdminClient.Verify(admin => admin.CreateTopicAsync(_testTopic, It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task SubscribeAsync_ShouldCreateSubscriptionIfNotExists()
        {
            // Arrange: subscription don't exist
            _mockAdminClient.Setup(admin => admin.SubscriptionExistsAsync(_testTopic, _testSubscription, It.IsAny<CancellationToken>()))
                .ReturnsAsync(Response.FromValue(false, Mock.Of<Response>()));

            // Act
            await _provider.SubscribeAsync(_testTopic, _testSubscription, (msg, token) => Task.CompletedTask, CancellationToken.None);

            // Assert: create-subscription was called
            _mockAdminClient.Verify(admin => admin.CreateSubscriptionAsync(_testTopic, _testSubscription, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task SubscribeAsync_ShouldStartProcessingMessages()
        {
            // Arrange: no-op handler
            Func<string, CancellationToken, Task> handler = (msg, token) => Task.CompletedTask;

            // Act
            await _provider.SubscribeAsync(_testTopic, _testSubscription, handler, CancellationToken.None);

            // Assert: start-processing was called
            _mockProcessor.Verify(processor => processor.StartProcessingAsync(It.IsAny<CancellationToken>()), Times.Once);
        }
        
        [Fact]
        public async Task PublishAsync_ShouldSendMessageToTopic()
        {
            // Arrange
            var testMessage = "Hello, Azure Service Bus!";

            // Act
            await _provider.PublishAsync(_testTopic, testMessage, CancellationToken.None);

            // Assert: sender was called with the message
            _mockSender.Verify(sender => sender.SendMessageAsync(It.Is<ServiceBusMessage>(
                    msg => Encoding.UTF8.GetString(msg.Body.ToArray()) == testMessage), 
                It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}
