using Microsoft.Extensions.Logging;
using Moq;
using Xunit;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using OpenDDD.Infrastructure.Events.Kafka;
using OpenDDD.Infrastructure.Events.Kafka.Factories;

namespace OpenDDD.Tests.Infrastructure.Events.Kafka
{
    public class KafkaMessagingProviderTests
    {
        private readonly Mock<IProducer<Null, string>> _mockProducer;
        private readonly Mock<IAdminClient> _mockAdminClient;
        private readonly Mock<KafkaConsumerFactory> _mockConsumerFactory;
        private readonly Mock<IConsumer<Ignore, string>> _mockConsumer;
        private readonly Mock<ILogger<KafkaMessagingProvider>> _mockLogger;
        private readonly KafkaMessagingProvider _provider;
        private const string BootstrapServers = "localhost:9092";
        private const string Topic = "test-topic";
        private const string Message = "Hello, Kafka!";
        private const string ConsumerGroup = "test-group";

        public KafkaMessagingProviderTests()
        {
            _mockProducer = new Mock<IProducer<Null, string>>();
            _mockAdminClient = new Mock<IAdminClient>();
            _mockConsumerFactory = new Mock<KafkaConsumerFactory>(BootstrapServers);
            _mockConsumer = new Mock<IConsumer<Ignore, string>>();
            _mockLogger = new Mock<ILogger<KafkaMessagingProvider>>();
            
            _provider = new KafkaMessagingProvider(
                BootstrapServers,
                _mockAdminClient.Object,
                _mockProducer.Object,
                _mockConsumerFactory.Object,
                autoCreateTopics: true,
                _mockLogger.Object);
            
            // Mock factory to always return same consumer
            _mockConsumerFactory
                .Setup(f => f.Create(It.IsAny<string>()))
                .Returns(_mockConsumer.Object);

            // Mock metadata retrieval for topics
            var metadata = new Metadata(
                new List<BrokerMetadata>(),
                new List<TopicMetadata>(),
                -1,
                ""
            );

            _mockAdminClient
                .Setup(a => a.GetMetadata(Topic, It.IsAny<TimeSpan>()))
                .Returns(metadata);
        }
        
        [Theory]
        [InlineData(null, "adminClient", "producer", "consumerFactory", "logger")]
        [InlineData("bootstrapServers", null, "producer", "consumerFactory", "logger")]
        [InlineData("bootstrapServers", "adminClient", null, "consumerFactory", "logger")]
        [InlineData("bootstrapServers", "adminClient", "producer", null, "logger")]
        [InlineData("bootstrapServers", "adminClient", "producer", "consumerFactory", null)]
        public void Constructor_ShouldThrowException_WhenDependenciesAreNull(
            string? bootstrapServers, string? adminClient, string? producer, string? consumerFactory, string? logger)
        {
            var bs = bootstrapServers is null ? null! : BootstrapServers;
            var mockAdmin = adminClient is null ? null! : _mockAdminClient.Object;
            var mockProducer = producer is null ? null! : _mockProducer.Object;
            var mockConsumerFactory = consumerFactory is null ? null! : _mockConsumerFactory.Object;
            var mockLogger = logger is null ? null! : _mockLogger.Object;

            Assert.Throws<ArgumentNullException>(() =>
                new KafkaMessagingProvider(bs, mockAdmin, mockProducer, mockConsumerFactory, true, mockLogger));
        }
        
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task SubscribeAsync_ShouldThrowException_WhenTopicIsInvalid(string invalidTopic)
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _provider.SubscribeAsync(invalidTopic, ConsumerGroup, (msg, token) => Task.CompletedTask, CancellationToken.None));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task SubscribeAsync_ShouldThrowException_WhenConsumerGroupIsInvalid(string invalidConsumerGroup)
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _provider.SubscribeAsync(Topic, invalidConsumerGroup, (msg, token) => Task.CompletedTask, CancellationToken.None));
        }

        [Fact]
        public async Task SubscribeAsync_ShouldThrowException_WhenHandlerIsNull()
        {
            await Assert.ThrowsAsync<ArgumentNullException>(() =>
                _provider.SubscribeAsync(Topic, ConsumerGroup, null!, CancellationToken.None));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task PublishAsync_ShouldThrowException_WhenTopicIsInvalid(string invalidTopic)
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _provider.PublishAsync(invalidTopic, Message, CancellationToken.None));
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public async Task PublishAsync_ShouldThrowException_WhenMessageIsInvalid(string invalidMessage)
        {
            await Assert.ThrowsAsync<ArgumentException>(() =>
                _provider.PublishAsync(Topic, invalidMessage, CancellationToken.None));
        }

        [Fact]
        public async Task PublishAsync_ShouldCall_ProduceAsync()
        {
            // Arrange
            var mockDeliveryResult = new DeliveryResult<Null, string>();
            _mockProducer
                .Setup(p => p.ProduceAsync(It.IsAny<string>(), It.IsAny<Message<Null, string>>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(mockDeliveryResult);

            // Act
            await _provider.PublishAsync(Topic, Message, CancellationToken.None);

            // Assert
            _mockProducer.Verify(p => p.ProduceAsync(
                Topic,
                It.Is<Message<Null, string>>(m => m.Value == Message),
                It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task PublishAsync_ShouldCall_CreateTopicIfNotExists_WhenAutoCreateTopicsIsEnabled()
        {
            // Act
            await _provider.PublishAsync(Topic, Message, CancellationToken.None);

            // Assert
            _mockAdminClient.Verify(a => a.CreateTopicsAsync(It.IsAny<IEnumerable<TopicSpecification>>(), null), Times.Once);
        }

        [Fact]
        public async Task SubscribeAsync_ShouldCreateConsumer_AndSubscribeToTopic()
        {
            // Act
            await _provider.SubscribeAsync(Topic, ConsumerGroup, async (_, _) => await Task.CompletedTask, CancellationToken.None);

            // Assert
            _mockConsumer.Verify(c => c.Subscribe(Topic), Times.Once);
        }
        
        [Fact]
        public async Task SubscribeAsync_ShouldProcessReceivedMessages()
        {
            // Arrange
            _mockConsumer
                .SetupSequence(c => c.Consume(It.IsAny<CancellationToken>()))
                .Returns(new ConsumeResult<Ignore, string>
                {
                    Message = new Message<Ignore, string> { Value = Message }
                })
                .Throws(new OperationCanceledException()); // Stop after the first message

            var messageReceived = new TaskCompletionSource<bool>();

            // Act: Start the subscription
            await _provider.SubscribeAsync(Topic, ConsumerGroup, async (msg, _) =>
            {
                if (msg == Message) messageReceived.SetResult(true);
                await Task.CompletedTask;
            }, CancellationToken.None);

            // Ensure the background task has enough time to consume the message
            await Task.Delay(100); 

            // Assert: Check if the message was received
            Assert.True(await messageReceived.Task.WaitAsync(TimeSpan.FromSeconds(2)), "Message handler was not called.");

            // Ensure commit is called after processing
            _mockConsumer.Verify(c => c.Commit(It.IsAny<ConsumeResult<Ignore, string>>()), Times.Once);
        }
        
        [Fact]
        public async Task SubscribeAsync_ShouldHandleConsumerExceptionsGracefully()
        {
            // Arrange
            _mockConsumer.Setup(c => c.Consume(It.IsAny<CancellationToken>()))
                .Throws(new KafkaException(ErrorCode.Local_Transport));

            // Act
            await _provider.SubscribeAsync(Topic, ConsumerGroup, async (_, _) => await Task.CompletedTask, CancellationToken.None);
            
            // Ensure the background task has enough time to consume the message
            await Task.Delay(100);

            // Assert
            _mockLogger.Verify(
                l => l.Log(LogLevel.Error, It.IsAny<EventId>(), It.IsAny<It.IsAnyType>(), It.IsAny<KafkaException>(), It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                Times.Once);
        }

        [Fact]
        public async Task DisposeAsync_ShouldDisposeAllConsumers_AndKafkaClients()
        {
            // Arrange
            await _provider.SubscribeAsync(Topic, ConsumerGroup, async (_, _) => await Task.CompletedTask, CancellationToken.None);
            
            // Ensure the background task has enough time to start
            await Task.Delay(100);

            // Act
            await _provider.DisposeAsync();

            // Assert
            _mockConsumer.Verify(c => c.Close(), Times.Once);
            _mockConsumer.Verify(c => c.Dispose(), Times.Once);
            _mockProducer.Verify(p => p.Dispose(), Times.Once);
            _mockAdminClient.Verify(a => a.Dispose(), Times.Once);
        }
    }
}
