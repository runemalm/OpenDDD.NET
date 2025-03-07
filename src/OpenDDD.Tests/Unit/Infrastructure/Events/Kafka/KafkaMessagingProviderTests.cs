using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Moq;
using OpenDDD.Infrastructure.Events.Kafka;
using OpenDDD.Infrastructure.Events.Kafka.Factories;
using OpenDDD.Tests.Base;

namespace OpenDDD.Tests.Unit.Infrastructure.Events.Kafka
{
    public class KafkaMessagingProviderTests : UnitTests
    {
        private readonly Mock<IProducer<Null, string>> _mockProducer;
        private readonly Mock<IAdminClient> _mockAdminClient;
        private readonly Mock<IKafkaConsumerFactory> _mockConsumerFactory;
        private readonly Mock<IConsumer<Ignore, string>> _mockConsumer;
        private readonly Mock<ILogger<KafkaMessagingProvider>> _mockLogger;
        private readonly Mock<ILogger<KafkaConsumer>> _mockConsumerLogger;
        private readonly KafkaMessagingProvider _provider;
        private const string BootstrapServers = "localhost:9092";
        private const string Topic = "test-topic";
        private const string Message = "Hello, Kafka!";
        private const string ConsumerGroup = "test-group";

        public KafkaMessagingProviderTests()
        {
            _mockProducer = new Mock<IProducer<Null, string>>();
            _mockAdminClient = new Mock<IAdminClient>();
            _mockConsumerFactory = new Mock<IKafkaConsumerFactory>();
            _mockConsumer = new Mock<IConsumer<Ignore, string>>();
            _mockLogger = new Mock<ILogger<KafkaMessagingProvider>>();
            _mockConsumerLogger = new Mock<ILogger<KafkaConsumer>>();
            
            _provider = new KafkaMessagingProvider(
                _mockAdminClient.Object,
                _mockProducer.Object,
                _mockConsumerFactory.Object,
                autoCreateTopics: true,
                _mockLogger.Object);
            
            _mockConsumerFactory
                .Setup(f => f.Create(It.IsAny<string>()))
                .Returns((string consumerGroup) =>
                {
                    var kafkaConsumer = new KafkaConsumer(_mockConsumer.Object, consumerGroup, _mockConsumerLogger.Object);
                    return kafkaConsumer;
                });

            var metadata = new Metadata(
                new List<BrokerMetadata> { new(1, "localhost", 9092) },
                new List<TopicMetadata> { new(Topic, new List<PartitionMetadata>(), ErrorCode.NoError) }, // Ensure topic exists
                -1,
                ""
            );
            _mockAdminClient
                .Setup(a => a.GetMetadata(It.IsAny<TimeSpan>()))
                .Returns(metadata);
        }
        
        [Theory]
        [InlineData(null, "producer", "consumerFactory", "logger")]
        [InlineData("adminClient", null, "consumerFactory", "logger")]
        [InlineData("adminClient", "producer", null, "logger")]
        [InlineData("adminClient", "producer", "consumerFactory", null)]
        public void Constructor_ShouldThrowException_WhenDependenciesAreNull(
            string? adminClient, string? producer, string? consumerFactory, string? logger)
        {
            var mockAdmin = adminClient is null ? null! : _mockAdminClient.Object;
            var mockProducer = producer is null ? null! : _mockProducer.Object;
            var mockConsumerFactory = consumerFactory is null ? null! : _mockConsumerFactory.Object;
            var mockLogger = logger is null ? null! : _mockLogger.Object;

            Assert.Throws<ArgumentNullException>(() =>
                new KafkaMessagingProvider(mockAdmin, mockProducer, mockConsumerFactory, true, mockLogger));
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
    }
}
