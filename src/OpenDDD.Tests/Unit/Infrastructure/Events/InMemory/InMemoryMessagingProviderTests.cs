using Microsoft.Extensions.Logging;
using Moq;
using OpenDDD.Infrastructure.Events.InMemory;
using OpenDDD.Tests.Base;

namespace OpenDDD.Tests.Unit.Infrastructure.Events.InMemory
{
    public class InMemoryMessagingProviderTests : UnitTests
    {
        private readonly Mock<ILogger<InMemoryMessagingProvider>> _mockLogger;
        private readonly InMemoryMessagingProvider _messagingProvider;
        private const string Topic = "TestTopic";
        private const string ConsumerGroup = "TestGroup";
        private const string Message = "Hello, InMemory!";

        public InMemoryMessagingProviderTests()
        {
            _mockLogger = new Mock<ILogger<InMemoryMessagingProvider>>();
            _messagingProvider = new InMemoryMessagingProvider(_mockLogger.Object);
        }

        // Constructor validation tests
        [Fact]
        public void Constructor_ShouldThrowException_WhenLoggerIsNull()
        {
            Assert.Throws<ArgumentNullException>(() => new InMemoryMessagingProvider(null!));
        }

        // SubscribeAsync validation tests
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task SubscribeAsync_ShouldThrowException_WhenTopicIsInvalid(string invalidTopic)
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _messagingProvider.SubscribeAsync(invalidTopic, ConsumerGroup, (msg, token) => Task.CompletedTask,
                    CancellationToken.None));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task SubscribeAsync_ShouldThrowException_WhenConsumerGroupIsInvalid(string invalidConsumerGroup)
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _messagingProvider.SubscribeAsync(Topic, invalidConsumerGroup, (msg, token) => Task.CompletedTask,
                    CancellationToken.None));
        }

        [Fact]
        public async Task SubscribeAsync_ShouldThrowException_WhenHandlerIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _messagingProvider.SubscribeAsync(Topic, ConsumerGroup, null!, CancellationToken.None));
        }

        // PublishAsync validation tests
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task PublishAsync_ShouldThrowException_WhenTopicIsInvalid(string invalidTopic)
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _messagingProvider.PublishAsync(invalidTopic, Message, CancellationToken.None));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task PublishAsync_ShouldThrowException_WhenMessageIsInvalid(string invalidMessage)
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _messagingProvider.PublishAsync(Topic, invalidMessage, CancellationToken.None));
        }
    }
}