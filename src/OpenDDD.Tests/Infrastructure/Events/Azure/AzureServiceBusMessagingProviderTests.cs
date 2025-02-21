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
    }
}
