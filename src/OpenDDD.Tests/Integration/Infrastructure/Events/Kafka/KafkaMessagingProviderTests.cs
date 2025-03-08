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
        private readonly CancellationTokenSource _cts = new(TimeSpan.FromSeconds(120));

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

        [Fact]
        public async Task AutoCreateTopic_ShouldCreateTopicOnSubscribe_WhenSettingEnabled()
        {
            // Arrange
            var topicName = $"test-topic-{Guid.NewGuid()}";
            var consumerGroup = "test-consumer-group";
        
            var metadata = _adminClient.GetMetadata(TimeSpan.FromSeconds(5));
            metadata.Topics.Any(t => t.Topic == topicName).Should().BeFalse();
        
            // Act
            await _messagingProvider.SubscribeAsync(topicName, consumerGroup, async (msg, token) => 
                await Task.CompletedTask, _cts.Token);
        
            var timeout = TimeSpan.FromSeconds(10);
            var pollingInterval = TimeSpan.FromMilliseconds(500);
            var startTime = DateTime.UtcNow;
        
            bool topicExists = false;
            while (DateTime.UtcNow - startTime < timeout)
            {
                metadata = _adminClient.GetMetadata(TimeSpan.FromSeconds(5));
                if (metadata.Topics.Any(t => t.Topic == topicName))
                {
                    topicExists = true;
                    break;
                }
                await Task.Delay(pollingInterval);
            }
        
            // Assert
            topicExists.Should().BeTrue("Kafka should create the topic automatically when subscribing.");
        }
        
        [Fact]
        public async Task AutoCreateTopic_ShouldNotCreateTopicOnSubscribe_WhenSettingDisabled()
        {
            // Arrange
            var topicName = $"test-topic-{Guid.NewGuid()}";
            var consumerGroup = "test-consumer-group";
        
            var metadata = _adminClient.GetMetadata(TimeSpan.FromSeconds(5));
            metadata.Topics.Any(t => t.Topic == topicName).Should().BeFalse("The topic should not exist before subscribing.");
            
            var messagingProvider = new KafkaMessagingProvider(
                _adminClient,
                _producer,
                _consumerFactory,
                autoCreateTopics: false,
                _logger);
        
            // Act & Assert
            var exception = await Assert.ThrowsAsync<InvalidOperationException>(async () =>
            {
                await messagingProvider.SubscribeAsync(topicName, consumerGroup, async (msg, token) => 
                    await Task.CompletedTask, _cts.Token);
            });
        
            exception.Message.Should().Be($"Topic '{topicName}' does not exist. Enable 'autoCreateTopics' to create topics automatically.");
        }
        
        [Fact]
        public async Task AtLeastOnceGurantee_ShouldDeliverToLateSubscriber_WhenSubscribedBefore()
        {
            // Arrange
            var topicName = $"test-topic-{Guid.NewGuid()}";
            var consumerGroup = $"test-consumer-group-{Guid.NewGuid()}";
            var messageToSend = "Persistent Message Test";
            var firstSubscriberReceived = new TaskCompletionSource<bool>();
            var lateSubscriberReceived = new TaskCompletionSource<bool>();
            ConcurrentBag<string> _receivedMessages = new();
        
            var firstSubscription = await _messagingProvider.SubscribeAsync(topicName, consumerGroup, async (msg, token) =>
            {
                firstSubscriberReceived.SetResult(true);
            }, _cts.Token);
            
            await WaitForKafkaConsumerGroupStable(consumerGroup, _cts.Token);
            
            await Task.Delay(500, _cts.Token);
            
            await _messagingProvider.PublishAsync(topicName, messageToSend, _cts.Token);
            
            await firstSubscriberReceived.Task.WaitAsync(TimeSpan.FromSeconds(30));
            
            await Task.Delay(500, _cts.Token);
        
            await _messagingProvider.UnsubscribeAsync(firstSubscription, _cts.Token);
            
            await Task.Delay(500, _cts.Token);
            
            await _messagingProvider.PublishAsync(topicName, messageToSend, _cts.Token);
            await Task.Delay(5000, _cts.Token);
            
            // Late subscriber
            await _messagingProvider.SubscribeAsync(topicName, consumerGroup, async (msg, token) =>
            {
                _receivedMessages.Add(msg);
                lateSubscriberReceived.TrySetResult(true);
            }, _cts.Token);
            
            await WaitForKafkaConsumerGroupStable(consumerGroup, _cts.Token);
            
            await Task.Delay(500, _cts.Token);
            
            await lateSubscriberReceived.Task.WaitAsync(TimeSpan.FromSeconds(30));
        
            // Assert
            _receivedMessages.Should().Contain(messageToSend);
        }
        
        [Fact]
        public async Task AtLeastOnceGurantee_ShouldNotDeliverToLateSubscriber_WhenNotSubscribedBefore()
        {
            // Arrange
            var topicName = $"test-topic-{Guid.NewGuid()}";
            var consumerGroup = "test-consumer-group";
            var messageToSend = "Non-Persistent Message Test";
            ConcurrentBag<string> _receivedMessages = new();
        
            // Act
            await _messagingProvider.PublishAsync(topicName, messageToSend, _cts.Token);
            await Task.Delay(2000);
        
            // Late subscriber
            await _messagingProvider.SubscribeAsync(topicName, consumerGroup, async (msg, token) =>
            {
                _receivedMessages.Add(msg);
            }, _cts.Token);
        
            await WaitForKafkaConsumerGroupStable(consumerGroup, _cts.Token);

            await Task.Delay(5000);
        
            // Assert
            _receivedMessages.Should().NotContain(messageToSend);
        }
        
        [Fact]
        public async Task AtLeastOnceGurantee_ShouldRedeliverLater_WhenMessageNotAcked()
        {
            // Arrange
            var topicName = $"test-topic-{Guid.NewGuid()}";
            var consumerGroup = "test-consumer-group";
            var messageToSend = "Redelivery Test";
            ConcurrentBag<string> _receivedMessages = new();
        
            async Task FaultyHandler(string msg, CancellationToken token)
            {
                _receivedMessages.Add(msg);
                throw new Exception("Simulated consumer crash before acknowledgment.");
            }
        
            await _messagingProvider.SubscribeAsync(topicName, consumerGroup, FaultyHandler, _cts.Token);

            await WaitForKafkaConsumerGroupStable(consumerGroup, _cts.Token);
        
            // Act
            await _messagingProvider.PublishAsync(topicName, messageToSend, _cts.Token);
        
            for (int i = 0; i < 300; i++)
            {
                if (_receivedMessages.Count > 1) break;
                await Task.Delay(1000);
            }
        
            // Assert
            _receivedMessages.Count.Should().BeGreaterThan(1);
        }
        
        [Fact]
        public async Task CompetingConsumers_ShouldDeliverOnlyOnce_WhenMultipleConsumersInGroup()
        {
            // Arrange
            var topicName = $"test-topic-{Guid.NewGuid()}";
            var consumerGroup = "test-consumer-group";
            var receivedMessages = new ConcurrentDictionary<Guid, int>(); // Track messages per consumer
            var messageToSend = "Competing Consumer Test";
            var messageProcessed = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var consumerIds = new ConcurrentBag<Guid>();

            async Task MessageHandler(string msg, CancellationToken token)
            {
                var consumerId = Guid.NewGuid();

                receivedMessages.AddOrUpdate(consumerId, 1, (_, count) => count + 1);
                consumerIds.Add(consumerId);

                _logger.LogDebug($"Consumer {consumerId} received a message.");

                messageProcessed.TrySetResult(true);
            }

            await _messagingProvider.SubscribeAsync(topicName, consumerGroup, MessageHandler, _cts.Token);
            await _messagingProvider.SubscribeAsync(topicName, consumerGroup, MessageHandler, _cts.Token);

            await WaitForKafkaConsumerGroupStable(consumerGroup, _cts.Token);

            // Act
            await _messagingProvider.PublishAsync(topicName, messageToSend, _cts.Token);

            try
            {
                await messageProcessed.Task.WaitAsync(TimeSpan.FromSeconds(5));
            }
            catch (TimeoutException)
            {
                _logger.LogDebug("Timed out waiting for consumer to receive the message.");
                Assert.Fail("No consumer received the message.");
            }

            await Task.Delay(5000);

            // Assert
            receivedMessages.Count.Should().Be(1, 
                $"Expected only one consumer to receive the message, but {receivedMessages.Count} consumers received it.");
        }
        
        [Fact]
        public async Task CompetingConsumers_ShouldDistributeMessages_WhenMultipleConsumersInGroup()
        {
            // Arrange
            var topicName = $"test-topic-{Guid.NewGuid()}";
            var consumerGroup = "test-consumer-group";
            var totalMessages = 100;
            var numConsumers = 2;
            var variancePercentage = 0.2;
            var perConsumerMessageCount = new ConcurrentDictionary<Guid, int>(); // Track messages per consumer
            var allMessagesProcessed = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            async Task CreateConsumer()
            {
                var consumerId = Guid.NewGuid();
                _logger.LogDebug($"Creating consumer with unique ID: {consumerId}");

                async Task MessageHandler(string msg, CancellationToken token)
                {
                    perConsumerMessageCount.AddOrUpdate(consumerId, 1, (_, count) => count + 1);
                    _logger.LogDebug($"Consumer {consumerId} received a message.");

                    if (perConsumerMessageCount.Values.Sum() >= totalMessages)
                    {
                        allMessagesProcessed.TrySetResult(true);
                    }
                }

                await _messagingProvider.SubscribeAsync(topicName, consumerGroup, MessageHandler, _cts.Token);
            }

            for (int i = 0; i < numConsumers; i++)
            {
                await CreateConsumer();
            }

            await WaitForKafkaConsumerGroupStable(consumerGroup, _cts.Token);

            // Act
            for (int i = 0; i < totalMessages; i++)
            {
                await _messagingProvider.PublishAsync(topicName, "Test Message", _cts.Token);
            }

            try
            {
                await allMessagesProcessed.Task.WaitAsync(TimeSpan.FromSeconds(10));
            }
            catch (TimeoutException)
            {
                _logger.LogDebug("Timed out waiting for consumers to receive all messages.");
                Assert.Fail($"Consumers only processed {perConsumerMessageCount.Values.Sum()} of {totalMessages} messages.");
            }

            // Assert
            var messageCounts = perConsumerMessageCount.Values.ToList();
            var expectedPerConsumer = totalMessages / numConsumers;
            var variance = (int)(expectedPerConsumer * variancePercentage);
            var minAllowed = expectedPerConsumer - variance;
            var maxAllowed = expectedPerConsumer + variance;

            foreach (var count in messageCounts)
            {
                Assert.InRange(count, minAllowed, maxAllowed);
            }
        }
        
        private async Task WaitForKafkaConsumerGroupStable(string consumerGroup, CancellationToken cancellationToken)
        {
            var maxAttempts = 30;
            for (int i = 0; i < maxAttempts; i++)
            {
                var groupInfo = _adminClient.ListGroups(TimeSpan.FromSeconds(5))
                    .FirstOrDefault(g => g.Group == consumerGroup);

                if (groupInfo?.State == "Stable")
                {
                    _logger.LogDebug("Consumer group {ConsumerGroup} is now stable.", consumerGroup);
                    return;
                }

                _logger.LogDebug("Waiting for consumer group {ConsumerGroup} to stabilize...", consumerGroup);
                await Task.Delay(500, cancellationToken);
            }

            throw new TimeoutException($"Consumer group {consumerGroup} did not stabilize in time.");
        }
    }
}
