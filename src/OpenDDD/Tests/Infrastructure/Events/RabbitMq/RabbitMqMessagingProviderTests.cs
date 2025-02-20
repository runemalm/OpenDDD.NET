using Microsoft.Extensions.Logging;
using Xunit;
using Moq;
using OpenDDD.Infrastructure.Events.RabbitMq;
using OpenDDD.Infrastructure.Events.RabbitMq.Factories;
using RabbitMQ.Client;

namespace OpenDDD.Tests.Infrastructure.Events.RabbitMq
{
    public class RabbitMqMessagingProviderTests
    {
        private readonly Mock<IConnectionFactory> _mockConnectionFactory;
        private readonly Mock<IRabbitMqConsumerFactory> _mockConsumerFactory;
        private readonly Mock<ILogger<RabbitMqMessagingProvider>> _mockLogger;
        private readonly RabbitMqMessagingProvider _provider;

        private const string TestTopic = "test-topic";
        private const string TestConsumerGroup = "test-group";

        public RabbitMqMessagingProviderTests()
        {
            _mockConnectionFactory = new Mock<IConnectionFactory>();
            _mockConsumerFactory = new Mock<IRabbitMqConsumerFactory>();
            _mockLogger = new Mock<ILogger<RabbitMqMessagingProvider>>();

            _provider = new RabbitMqMessagingProvider(
                _mockConnectionFactory.Object,
                _mockConsumerFactory.Object,
                _mockLogger.Object
            );
        }
        
        [Theory]
        [InlineData(null, "consumerFactory", "logger")] 
        [InlineData("connectionFactory", null, "logger")]
        [InlineData("connectionFactory", "consumerFactory", null)]
        public void Constructor_ShouldThrowException_WhenDependenciesAreNull(
            string? connectionFactory, string? consumerFactory, string? logger)
        {
            var mockConnectionFactory = connectionFactory is null ? null! : _mockConnectionFactory.Object;
            var mockConsumerFactory = consumerFactory is null ? null! : _mockConsumerFactory.Object;
            var mockLogger = logger is null ? null! : _mockLogger.Object;

            Assert.Throws<ArgumentNullException>(() =>
                new RabbitMqMessagingProvider(mockConnectionFactory, mockConsumerFactory, mockLogger));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task SubscribeAsync_ShouldThrowException_WhenTopicIsInvalid(string invalidTopic)
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _provider.SubscribeAsync(invalidTopic, TestConsumerGroup, (msg, token) => Task.CompletedTask, CancellationToken.None));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task SubscribeAsync_ShouldThrowException_WhenConsumerGroupIsInvalid(string invalidConsumerGroup)
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _provider.SubscribeAsync(TestTopic, invalidConsumerGroup, (msg, token) => Task.CompletedTask, CancellationToken.None));
        }

        [Fact]
        public async Task SubscribeAsync_ShouldThrowException_WhenHandlerIsNull()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _provider.SubscribeAsync(TestTopic, TestConsumerGroup, null!, CancellationToken.None));
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
                _provider.PublishAsync(TestTopic, invalidMessage, CancellationToken.None));
        }
    }
}
