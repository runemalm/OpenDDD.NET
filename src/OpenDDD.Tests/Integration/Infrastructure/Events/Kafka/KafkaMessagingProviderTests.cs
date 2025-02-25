using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;
using FluentAssertions;
using OpenDDD.Infrastructure.Events.Kafka;
using OpenDDD.Infrastructure.Events.Kafka.Factories;
using OpenDDD.Tests.Base;
using Confluent.Kafka;

namespace OpenDDD.Tests.Integration.Infrastructure.Events.Kafka
{
    [Collection("KafkaTests")]
    public class KafkaMessagingProviderTests : IntegrationTests, IAsyncLifetime
    {
        private readonly string _bootstrapServers;
        private readonly IAdminClient _adminClient;
        private readonly IProducer<Null, string> _producer;
        private readonly KafkaConsumerFactory _consumerFactory;
        private readonly ILogger<KafkaMessagingProvider> _logger;
        private readonly ILogger<KafkaConsumer> _consumerLogger;
        private readonly KafkaMessagingProvider _messagingProvider;
        private readonly CancellationTokenSource _cts = new();

        public KafkaMessagingProviderTests(ITestOutputHelper testOutputHelper) 
            : base(testOutputHelper, enableLogging: true)
        {
            _bootstrapServers = Environment.GetEnvironmentVariable("KAFKA_BOOTSTRAP_SERVERS")
                                ?? throw new InvalidOperationException("KAFKA_BOOTSTRAP_SERVERS is not set.");

            var adminClientConfig = new AdminClientConfig { BootstrapServers = _bootstrapServers };
            var producerConfig = new ProducerConfig { BootstrapServers = _bootstrapServers };

            _adminClient = new AdminClientBuilder(adminClientConfig).Build();
            _producer = new ProducerBuilder<Null, string>(producerConfig).Build();
            _logger = LoggerFactory.CreateLogger<KafkaMessagingProvider>();
            _consumerLogger = LoggerFactory.CreateLogger<KafkaConsumer>();
            _consumerFactory = new KafkaConsumerFactory(_bootstrapServers, _consumerLogger);

            _messagingProvider = new KafkaMessagingProvider(
                _bootstrapServers,
                _adminClient,
                _producer,
                _consumerFactory,
                autoCreateTopics: true,
                _logger);
        }

        public async Task InitializeAsync()
        {
            await CleanupTopicsAndConsumerGroupsAsync();
        }

        public async Task DisposeAsync()
        {
            await _cts.CancelAsync();
            await _messagingProvider.DisposeAsync();
        }
        
        private async Task CleanupTopicsAndConsumerGroupsAsync()
        {
            try
            {
                // Delete test topics
                var metadata = _adminClient.GetMetadata(TimeSpan.FromSeconds(5));
                var testTopics = metadata.Topics
                    .Where(t => t.Topic.StartsWith("test-topic-"))
                    .Select(t => t.Topic)
                    .ToList();

                if (testTopics.Any())
                {
                    await _adminClient.DeleteTopicsAsync(testTopics);
                    _logger.LogInformation("Deleted test topics: {Topics}", string.Join(", ", testTopics));
                }

                // Delete consumer groups
                var consumerGroups = _adminClient.ListGroups(TimeSpan.FromSeconds(5))
                    .Where(g => g.Group.StartsWith("test-consumer-group"))
                    .Select(g => g.Group)
                    .ToList();

                if (consumerGroups.Any())
                {
                    await _adminClient.DeleteGroupsAsync(consumerGroups);
                    _logger.LogInformation("Deleted test consumer groups: {Groups}", string.Join(", ", consumerGroups));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to clean up Kafka topics and consumer groups.");
            }
        }

        // [Fact]
        // public async Task AutoCreateTopic_ShouldCreateTopicOnSubscribe_WhenSettingEnabled()
        // {
        //     // Arrange
        //     var topicName = $"test-topic-{Guid.NewGuid()}";
        //     var consumerGroup = "test-consumer-group";
        //
        //     // Ensure topic does not exist
        //     var metadata = _adminClient.GetMetadata(TimeSpan.FromSeconds(5));
        //     metadata.Topics.Any(t => t.Topic == topicName).Should().BeFalse();
        //
        //     // Act
        //     await _messagingProvider.SubscribeAsync(topicName, consumerGroup, async (msg, token) => 
        //         await Task.CompletedTask, _cts.Token);
        //
        //     await Task.Delay(1000);
        //
        //     // Assert
        //     metadata = _adminClient.GetMetadata(TimeSpan.FromSeconds(5));
        //     metadata.Topics.Any(t => t.Topic == topicName).Should().BeTrue();
        // }

        // [Fact]
        // public async Task AutoCreateTopic_ShouldNotCreateTopicOnSubscribe_WhenSettingDisabled()
        // {
        //     // Arrange
        //     var topicName = $"test-topic-{Guid.NewGuid()}";
        //     var consumerGroup = "test-consumer-group";
        //
        //     var messagingProvider = new KafkaMessagingProvider(
        //         _bootstrapServers,
        //         _adminClient,
        //         _producer,
        //         _consumerFactory,
        //         autoCreateTopics: false,
        //         _logger);
        //
        //     // Act & Assert
        //     var exception = await Assert.ThrowsAsync<KafkaException>(async () =>
        //     {
        //         await messagingProvider.SubscribeAsync(topicName, consumerGroup, async (msg, token) => 
        //             await Task.CompletedTask, _cts.Token);
        //     });
        //
        //     exception.Message.Should().Contain($"Topic '{topicName}' does not exist.");
        // }
        //
        // [Fact]
        // public async Task AtLeastOnceGurantee_ShouldDeliverToLateSubscriber_WhenSubscribedBefore()
        // {
        //     // Arrange
        //     var topicName = $"test-topic-{Guid.NewGuid()}";
        //     var consumerGroup = $"test-consumer-group-{Guid.NewGuid()}";
        //     var messageToSend = "Persistent Message Test";
        //     var firstSubscriberReceived = new TaskCompletionSource<bool>();
        //     var lateSubscriberReceived = new TaskCompletionSource<bool>();
        //     ConcurrentBag<string> _receivedMessages = new();
        //
        //     await _messagingProvider.SubscribeAsync(topicName, consumerGroup, async (msg, token) =>
        //     {
        //         firstSubscriberReceived.SetResult(true);
        //     }, _cts.Token);
        //
        //     await Task.Delay(500);
        //     
        //     await _messagingProvider.PublishAsync(topicName, messageToSend, _cts.Token);
        //
        //     await firstSubscriberReceived.Task;
        //
        //     await _messagingProvider.UnsubscribeAsync(topicName, consumerGroup, _cts.Token);
        //     await Task.Delay(500);
        //
        //     // Late subscriber
        //     await _messagingProvider.SubscribeAsync(topicName, consumerGroup, async (msg, token) =>
        //     {
        //         _receivedMessages.Add(msg);
        //         lateSubscriberReceived.TrySetResult(true);
        //     }, _cts.Token);
        //     
        //     await _messagingProvider.PublishAsync(topicName, messageToSend, _cts.Token);
        //
        //     await lateSubscriberReceived.Task;
        //
        //     // Assert
        //     _receivedMessages.Should().Contain(messageToSend);
        // }
        //
        // [Fact]
        // public async Task AtLeastOnceGurantee_ShouldNotDeliverToLateSubscriber_WhenNotSubscribedBefore()
        // {
        //     // Arrange
        //     var topicName = $"test-topic-{Guid.NewGuid()}";
        //     var consumerGroup = "test-consumer-group";
        //     var messageToSend = "Non-Persistent Message Test";
        //     ConcurrentBag<string> _receivedMessages = new();
        //
        //     // Act: Publish message before any subscription
        //     await _messagingProvider.PublishAsync(topicName, messageToSend, _cts.Token);
        //     await Task.Delay(500);
        //
        //     // Late subscriber
        //     await _messagingProvider.SubscribeAsync(topicName, consumerGroup, async (msg, token) =>
        //     {
        //         _receivedMessages.Add(msg);
        //     }, _cts.Token);
        //
        //     await Task.Delay(1000);
        //
        //     // Assert
        //     _receivedMessages.Should().NotContain(messageToSend);
        // }
        //
        // [Fact]
        // public async Task AtLeastOnceGurantee_ShouldRedeliverLater_WhenMessageNotAcked()
        // {
        //     // Arrange
        //     var topicName = $"test-topic-{Guid.NewGuid()}";
        //     var consumerGroup = "test-consumer-group";
        //     var messageToSend = "Redelivery Test";
        //     ConcurrentBag<string> _receivedMessages = new();
        //
        //     async Task FaultyHandler(string msg, CancellationToken token)
        //     {
        //         _receivedMessages.Add(msg);
        //         throw new Exception("Simulated consumer crash before acknowledgment.");
        //     }
        //
        //     await _messagingProvider.SubscribeAsync(topicName, consumerGroup, FaultyHandler, _cts.Token);
        //     await Task.Delay(500);
        //
        //     // Act: Publish message
        //     await _messagingProvider.PublishAsync(topicName, messageToSend, _cts.Token);
        //
        //     // Wait for redelivery
        //     for (int i = 0; i < 300; i++)
        //     {
        //         if (_receivedMessages.Count > 1) break;
        //         await Task.Delay(1000); // Check every second
        //     }
        //
        //     // Assert: The message should be received multiple times due to reattempts
        //     _receivedMessages.Count.Should().BeGreaterThan(1);
        // }
        
        [Fact]
        public async Task CompetingConsumers_ShouldDeliverOnlyOnce_WhenMultipleConsumersInGroup()
        {
            // Arrange
            var topicName = $"test-topic-{Guid.NewGuid()}";
            var consumerGroup = "test-consumer-group";
            var receivedMessages = new ConcurrentDictionary<string, int>();
            var messageToSend = "Competing Consumer Test";
        
            async Task MessageHandler(string msg, CancellationToken token)
            {
                receivedMessages.AddOrUpdate("received", 1, (key, value) => value + 1);
            }
        
            // Multiple competing consumers in the same group
            await _messagingProvider.SubscribeAsync(topicName, consumerGroup, MessageHandler, _cts.Token);
            await _messagingProvider.SubscribeAsync(topicName, consumerGroup, MessageHandler, _cts.Token);
            await Task.Delay(500);
        
            // Act
            await _messagingProvider.PublishAsync(topicName, messageToSend, _cts.Token);
        
            // Wait for processing
            await Task.Delay(5000);
        
            // Assert: Only one of the competing consumers should receive the message
            receivedMessages.GetValueOrDefault("received", 0).Should().Be(1);
        }
        
        // [Fact]
        // public async Task CompetingConsumers_ShouldDistributeEvenly_WhenMultipleConsumersInGroup()
        // {
        //     // Arrange
        //     var topicName = $"test-topic-{Guid.NewGuid()}";
        //     var consumerGroup = "test-consumer-group";
        //     var receivedMessages = new ConcurrentDictionary<string, int>();
        //     var totalMessages = 10;
        //
        //     async Task MessageHandler(string msg, CancellationToken token)
        //     {
        //         receivedMessages.AddOrUpdate(msg, 1, (key, value) => value + 1);
        //     }
        //
        //     await _messagingProvider.SubscribeAsync(topicName, consumerGroup, MessageHandler, _cts.Token);
        //     await _messagingProvider.SubscribeAsync(topicName, consumerGroup, MessageHandler, _cts.Token);
        //     await Task.Delay(500);
        //
        //     // Act: Publish multiple messages
        //     for (int i = 0; i < totalMessages; i++)
        //     {
        //         await _messagingProvider.PublishAsync(topicName, $"Message {i}", _cts.Token);
        //     }
        //
        //     await Task.Delay(2000);
        //
        //     // Assert: Messages should be evenly distributed across consumers
        //     var messageCounts = receivedMessages.Values;
        //     var minReceived = messageCounts.Min();
        //     var maxReceived = messageCounts.Max();
        //
        //     Assert.True(maxReceived - minReceived <= 1,
        //         "Messages should be evenly distributed among competing consumers.");
        // }
    }
}
