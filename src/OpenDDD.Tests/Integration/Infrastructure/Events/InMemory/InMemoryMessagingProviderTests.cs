using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;
using FluentAssertions;
using OpenDDD.Infrastructure.Events.InMemory;
using OpenDDD.Tests.Base;

namespace OpenDDD.Tests.Integration.Infrastructure.Events.InMemory
{
    [Collection("InMemoryTests")]
    public class InMemoryMessagingProviderTests : IntegrationTests, IAsyncLifetime
    {
        private readonly ILogger<InMemoryMessagingProvider> _logger;
        private readonly InMemoryMessagingProvider _messagingProvider;
        private readonly CancellationTokenSource _cts = new(TimeSpan.FromSeconds(60));

        public InMemoryMessagingProviderTests(ITestOutputHelper testOutputHelper) 
            : base(testOutputHelper, enableLogging: true)
        {
            _logger = LoggerFactory.CreateLogger<InMemoryMessagingProvider>();
            _messagingProvider = new InMemoryMessagingProvider(_logger);
        }

        public Task InitializeAsync() => Task.CompletedTask;

        public async Task DisposeAsync()
        {
            _cts.Cancel();
            await _messagingProvider.DisposeAsync();
        }

        [Fact]
        public async Task AtLeastOnceGuarantee_ShouldDeliverToLateSubscriber_WhenSubscribedBefore()
        {
            // Arrange
            var topicName = $"test-topic-{Guid.NewGuid()}";
            var groupName = "test-subscription";
            var receivedMessages = new ConcurrentBag<string>();
            var messageToSend = "Persistent Message Test";
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            var lateSubscriberReceived = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            await _messagingProvider.SubscribeAsync(topicName, groupName, async (msg, token) =>
            {
                Assert.Fail("First subscription should not receive the message.");
            }, cts.Token);

            await Task.Delay(500);

            await _messagingProvider.UnsubscribeAsync(topicName, groupName, cts.Token);

            // Act
            await _messagingProvider.PublishAsync(topicName, messageToSend, cts.Token);

            await _messagingProvider.SubscribeAsync(topicName, groupName, async (msg, token) =>
            {
                receivedMessages.Add(msg);
                lateSubscriberReceived.TrySetResult(true);
            }, cts.Token);

            // Assert
            try
            {
                await lateSubscriberReceived.Task.WaitAsync(TimeSpan.FromSeconds(5));
            }
            catch (TimeoutException)
            {
                Assert.Fail($"Late subscriber did not receive the expected message '{messageToSend}' within 5 seconds.");
            }

            receivedMessages.Should().Contain(messageToSend, "The subscriber should receive messages published while it was previously subscribed.");
        }

        [Fact]
        public async Task AtLeastOnceGuarantee_ShouldNotDeliverToLateSubscriber_WhenNotSubscribedBefore()
        {
            // Arrange
            var topicName = "test-topic-no-late-subscriber";
            var consumerGroup = "test-consumer-group";
            var messageToSend = "Non-Persistent Message Test";
            ConcurrentBag<string> _receivedMessages = new();
        
            // Act
            await _messagingProvider.PublishAsync(topicName, messageToSend, _cts.Token);
            await Task.Delay(500);
        
            // Late subscriber
            await _messagingProvider.SubscribeAsync(topicName, consumerGroup, async (msg, token) =>
            {
                _receivedMessages.Add(msg);
            }, _cts.Token);
        
            await Task.Delay(2000);
        
            // Assert
            _receivedMessages.Should().NotContain(messageToSend);
        }

        [Fact]
        public async Task AtLeastOnceGuarantee_ShouldRedeliverLater_WhenMessageNotAcked()
        {
            // Arrange
            var topicName = "test-topic-redelivery";
            var consumerGroup = "test-consumer-group";
            var messageToSend = "Redelivery Test";
            ConcurrentBag<string> _receivedMessages = new();
        
            async Task FaultyHandler(string msg, CancellationToken token)
            {
                _receivedMessages.Add(msg);
                throw new Exception("Simulated consumer crash before acknowledgment.");
            }
        
            await _messagingProvider.SubscribeAsync(topicName, consumerGroup, FaultyHandler, _cts.Token);
        
            // Act
            await _messagingProvider.PublishAsync(topicName, messageToSend, _cts.Token);
        
            for (int i = 0; i < 20; i++)
            {
                if (_receivedMessages.Count > 1) break;
                await Task.Delay(500);
            }
        
            // Assert
            _receivedMessages.Count.Should().BeGreaterThan(1);
        }

        [Fact]
        public async Task CompetingConsumers_ShouldDeliverOnlyOnce_WhenMultipleConsumersInGroup()
        {
            // Arrange
            var topicName = "test-topic-competing-consumers";
            var consumerGroup = "test-consumer-group";
            var receivedMessages = new ConcurrentDictionary<string, int>();
            var messageToSend = "Competing Consumer Test";
            var allSubscribersProcessed = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
    
            async Task MessageHandler(string msg, CancellationToken token)
            {
                receivedMessages.AddOrUpdate("received", 1, (key, value) => value + 1);

                // If any consumer has received more than 1 message, fail immediately
                if (receivedMessages["received"] > 1)
                {
                    allSubscribersProcessed.TrySetException(new Exception("More than one consumer in the group received the message!"));
                }
                else
                {
                    allSubscribersProcessed.TrySetResult(true);
                }
            }

            // Subscribe multiple consumers in the same group
            await _messagingProvider.SubscribeAsync(topicName, consumerGroup, MessageHandler, _cts.Token);
            await _messagingProvider.SubscribeAsync(topicName, consumerGroup, MessageHandler, _cts.Token);

            // Act
            await _messagingProvider.PublishAsync(topicName, messageToSend, _cts.Token);

            try
            {
                await allSubscribersProcessed.Task.WaitAsync(TimeSpan.FromSeconds(5));
            }
            catch (TimeoutException)
            {
                Assert.Fail("Timed out waiting for message processing.");
            }

            // Assert: Only one consumer in the group should receive the message
            receivedMessages.GetValueOrDefault("received", 0).Should().Be(1);
        }

        [Fact]
        public async Task CompetingConsumers_ShouldDistributeEvenly_WhenMultipleConsumersInGroup()
        {
            // Arrange
            var topicName = "test-topic-even-distribution";
            var consumerGroup = "test-consumer-group";
            var totalMessages = 100;
            var numConsumers = 2;
            var variancePercentage = 0.1; // Allow 10% variance
            var perConsumerMessageCount = new ConcurrentDictionary<Guid, int>();
            var allMessagesProcessed = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            async Task CreateConsumer()
            {
                var consumerId = Guid.NewGuid();

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
    }
}
