using Microsoft.Extensions.Logging;
using FluentAssertions;
using Moq;
using Xunit;
using OpenDDD.Infrastructure.Events.InMemory;
using OpenDDD.Tests.Base;

namespace OpenDDD.Tests.Infrastructure.Events.InMemory
{
    public class InMemoryMessagingProviderTests : UnitTests
    {
        private readonly Mock<ILogger<InMemoryMessagingProvider>> _mockLogger;
        private readonly InMemoryMessagingProvider _messagingProvider;

        public InMemoryMessagingProviderTests()
        {
            _mockLogger = new Mock<ILogger<InMemoryMessagingProvider>>();
            _messagingProvider = new InMemoryMessagingProvider(_mockLogger.Object);
        }

        [Fact]
        public async Task SubscribeAsync_ShouldStoreMessageHandler_ForGivenTopicAndConsumerGroup()
        {
            // Arrange
            var topic = "TestTopic";
            var consumerGroup = "TestGroup";
            Func<string, CancellationToken, Task> handler = async (message, ct) => await Task.CompletedTask;

            // Act
            await _messagingProvider.SubscribeAsync(topic, consumerGroup, handler, CancellationToken.None);

            // Assert
            _mockLogger.Verify(
                log => log.Log(
                    LogLevel.Information,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Subscribed to topic: {topic} in listener group: {consumerGroup}")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()
                ),
                Times.Once
            );
        }

        [Fact]
        public async Task PublishAsync_ShouldInvokeSubscribedHandler()
        {
            // Arrange
            var topic = "TestTopic";
            var consumerGroup = "TestGroup";
            var receivedMessages = new List<string>();

            Func<string, CancellationToken, Task> handler = async (message, ct) =>
            {
                receivedMessages.Add(message);
                await Task.CompletedTask;
            };

            await _messagingProvider.SubscribeAsync(topic, consumerGroup, handler, CancellationToken.None);

            // Act
            await _messagingProvider.PublishAsync(topic, "Hello, World!", CancellationToken.None);

            // Allow some time for async handlers to execute
            await Task.Delay(100);

            // Assert
            receivedMessages.Should().ContainSingle()
                .Which.Should().Be("Hello, World!");
        }

        [Fact]
        public async Task PublishAsync_ShouldNotThrow_WhenNoSubscribersExist()
        {
            // Arrange
            var topic = "NonExistentTopic";

            // Act
            Func<Task> act = async () => await _messagingProvider.PublishAsync(topic, "Message", CancellationToken.None);

            // Assert
            await act.Should().NotThrowAsync();
        }

        [Fact]
        public async Task PublishAsync_ShouldInvokeAllHandlers_ForMultipleSubscriptions()
        {
            // Arrange
            var topic = "TestTopic";
            var consumerGroup1 = "Group1";
            var consumerGroup2 = "Group2";
            var receivedMessages1 = new List<string>();
            var receivedMessages2 = new List<string>();

            Func<string, CancellationToken, Task> handler1 = async (message, ct) =>
            {
                receivedMessages1.Add(message);
                await Task.CompletedTask;
            };

            Func<string, CancellationToken, Task> handler2 = async (message, ct) =>
            {
                receivedMessages2.Add(message);
                await Task.CompletedTask;
            };

            await _messagingProvider.SubscribeAsync(topic, consumerGroup1, handler1, CancellationToken.None);
            await _messagingProvider.SubscribeAsync(topic, consumerGroup2, handler2, CancellationToken.None);

            // Act
            await _messagingProvider.PublishAsync(topic, "Event 1", CancellationToken.None);

            // Allow time for async handlers
            await Task.Delay(100);

            // Assert
            receivedMessages1.Should().ContainSingle().Which.Should().Be("Event 1");
            receivedMessages2.Should().ContainSingle().Which.Should().Be("Event 1");
        }
        
        [Fact]
        public async Task PublishAsync_ShouldDeliverMessageToOnlyOneConsumer_InCompetingConsumerGroup()
        {
            // Arrange
            var topic = "TestTopic";
            var consumerGroup = "CompetingGroup";
            var receivedMessages1 = new List<string>();
            var receivedMessages2 = new List<string>();

            Func<string, CancellationToken, Task> handler1 = async (message, ct) =>
            {
                receivedMessages1.Add(message);
                await Task.CompletedTask;
            };

            Func<string, CancellationToken, Task> handler2 = async (message, ct) =>
            {
                receivedMessages2.Add(message);
                await Task.CompletedTask;
            };

            // Two consumers in the same group
            await _messagingProvider.SubscribeAsync(topic, consumerGroup, handler1, CancellationToken.None);
            await _messagingProvider.SubscribeAsync(topic, consumerGroup, handler2, CancellationToken.None);

            // Act
            await _messagingProvider.PublishAsync(topic, "Competing Message", CancellationToken.None);

            // Allow time for async handlers
            await Task.Delay(100);

            // Assert: Only one of the consumers should receive the message
            var totalMessagesReceived = receivedMessages1.Count + receivedMessages2.Count;
            totalMessagesReceived.Should().Be(1);
        }

        [Fact]
        public async Task PublishAsync_ShouldHandleException_WhenHandlerThrows()
        {
            // Arrange
            var topic = "TestTopic";
            var consumerGroup = "TestGroup";

            Func<string, CancellationToken, Task> failingHandler = async (message, ct) =>
            {
                await Task.CompletedTask;
                throw new InvalidOperationException("Handler error");
            };

            await _messagingProvider.SubscribeAsync(topic, consumerGroup, failingHandler, CancellationToken.None);

            // Act
            await _messagingProvider.PublishAsync(topic, "Test Message", CancellationToken.None);

            // Allow some time for async handlers to execute
            await Task.Delay(100);

            // Assert
            _mockLogger.Verify(
                log => log.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains($"Error in handler for topic '{topic}': Handler error")),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception, string>>()
                ),
                Times.Once
            );
        }
    }
}
