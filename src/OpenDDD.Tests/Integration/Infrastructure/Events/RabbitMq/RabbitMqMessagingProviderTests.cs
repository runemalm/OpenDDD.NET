using System.Collections.Concurrent;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Xunit.Abstractions;
using OpenDDD.Infrastructure.Events.RabbitMq;
using OpenDDD.Infrastructure.Events.RabbitMq.Factories;
using OpenDDD.Tests.Base;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;

namespace OpenDDD.Tests.Integration.Infrastructure.Events.RabbitMq
{
    [Collection("RabbitMqTests")]
    public class RabbitMqMessagingProviderTests : IntegrationTests, IAsyncLifetime
    {
        private readonly RabbitMqMessagingProvider _messagingProvider;
        private readonly IConnectionFactory _connectionFactory;
        private readonly IRabbitMqConsumerFactory _consumerFactory;
        private readonly ILogger<RabbitMqMessagingProvider> _logger;
        private IConnection? _connection;
        private IChannel? _channel;
        private readonly CancellationTokenSource _cts = new(TimeSpan.FromSeconds(60));
        
        private readonly string _testTopic = "OpenDddTestTopic";
        private readonly string _testConsumerGroup = "OpenDddTestGroup";

        public RabbitMqMessagingProviderTests(ITestOutputHelper testOutputHelper)
            : base(testOutputHelper, enableLogging: true)
        {
            _logger = LoggerFactory.CreateLogger<RabbitMqMessagingProvider>();

            _connectionFactory = new ConnectionFactory
            {
                HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost",
                Port = int.Parse(Environment.GetEnvironmentVariable("RABBITMQ_PORT") ?? "5672"),
                UserName = Environment.GetEnvironmentVariable("RABBITMQ_USERNAME") ?? "guest",
                Password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? "guest",
                VirtualHost = Environment.GetEnvironmentVariable("RABBITMQ_VHOST") ?? "/"
            };

            _consumerFactory = new RabbitMqConsumerFactory(_logger);
            _messagingProvider = new RabbitMqMessagingProvider(
                _connectionFactory, 
                _consumerFactory, 
                autoCreateTopics: true,
                _logger);
        }

        public async Task InitializeAsync()
        {
            await EnsureConnectionAndChannelOpenAsync();
            await CleanupExchangesAndQueuesAsync();
        }

        public async Task DisposeAsync()
        {
            await CleanupExchangesAndQueuesAsync();

            if (_channel is not null)
            {
                await _channel.CloseAsync();
                await _channel.DisposeAsync();
            }

            if (_connection is not null)
            {
                await _connection.CloseAsync();
                await _connection.DisposeAsync();
            }
            
            _cts.Cancel();
            await _messagingProvider.DisposeAsync();
        }
        
        private async Task VerifyExchangeAndQueueDoNotExist()
        {
            try
            {
                await _channel!.ExchangeDeclarePassiveAsync(_testTopic, CancellationToken.None);
                Assert.Fail($"Exchange '{_testTopic}' already exists before test.");
            }
            catch (OperationInterruptedException ex) when (ex.ShutdownReason?.ReplyCode == 404)
            {
                // Expected: Exchange does not exist
            }
    
            await EnsureConnectionAndChannelOpenAsync();

            try
            {
                await _channel!.QueueDeclarePassiveAsync($"{_testConsumerGroup}.{_testTopic}", CancellationToken.None);
                Assert.Fail($"Queue '{_testConsumerGroup}.{_testTopic}' already exists before test.");
            }
            catch (OperationInterruptedException ex) when (ex.ShutdownReason?.ReplyCode == 404)
            {
                // Expected: Queue does not exist
            }
    
            await EnsureConnectionAndChannelOpenAsync();
        }

        private async Task EnsureConnectionAndChannelOpenAsync()
        {
            if (_connection is null || !_connection.IsOpen)
            {
                _connection = await _connectionFactory.CreateConnectionAsync(CancellationToken.None);
            }

            if (_channel is null || !_channel.IsOpen)
            {
                _channel = await _connection.CreateChannelAsync(null, CancellationToken.None);
            }
        }

        private async Task CleanupExchangesAndQueuesAsync()
        {
            try
            {
                await _channel!.ExchangeDeleteAsync(_testTopic, ifUnused: false, cancellationToken: CancellationToken.None);
            }
            catch (OperationInterruptedException) { /* Exchange does not exist */ }

            try
            {
                await _channel!.QueueDeleteAsync($"{_testConsumerGroup}.{_testTopic}", ifUnused: false, ifEmpty: false, cancellationToken: CancellationToken.None);
            }
            catch (OperationInterruptedException) { /* Queue does not exist */ }
        }
        
        [Fact]
        public async Task AutoCreateTopic_ShouldCreateTopicOnSubscribe_WhenSettingEnabled()
        {
            // Arrange
            var topicName = $"test-topic-{Guid.NewGuid()}";
            var consumerGroup = "test-consumer-group";

            var messagingProvider = new RabbitMqMessagingProvider(
                _connectionFactory,
                _consumerFactory,
                autoCreateTopics: true, // Auto-create enabled
                _logger);

            var exchangeExistsBefore = await ExchangeExistsAsync(topicName, _cts.Token);
            exchangeExistsBefore.Should().BeFalse("The exchange should not exist before subscribing.");

            // Act
            await messagingProvider.SubscribeAsync(topicName, consumerGroup, async (msg, token) =>
                await Task.CompletedTask, _cts.Token);
            
            var timeout = TimeSpan.FromSeconds(30);
            var pollingInterval = TimeSpan.FromMilliseconds(500);
            var startTime = DateTime.UtcNow;

            bool exchangeExists = false;
            while (DateTime.UtcNow - startTime < timeout)
            {
                if (await ExchangeExistsAsync(topicName, _cts.Token))
                {
                    exchangeExists = true;
                    break;
                }
                await Task.Delay(pollingInterval, _cts.Token);
            }

            // Assert
            exchangeExists.Should().BeTrue("RabbitMQ should create the exchange automatically when subscribing.");
        }

        [Fact]
        public async Task AutoCreateTopic_ShouldNotCreateTopicOnSubscribe_WhenSettingDisabled()
        {
            // Arrange
            var topicName = $"test-topic-{Guid.NewGuid()}";
            var consumerGroup = "test-consumer-group";
            
            var exchangeExistsBefore = await ExchangeExistsAsync(topicName, _cts.Token);
            exchangeExistsBefore.Should().BeFalse("The exchange should not exist before subscribing.");

            var messagingProvider = new RabbitMqMessagingProvider(
                _connectionFactory,
                _consumerFactory,
                autoCreateTopics: false, // Auto-create disabled
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
            var receivedMessages = new ConcurrentBag<string>();
            var messageToSend = "Persistent Message Test";
            
            var lateSubscriberReceived = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

            var firstSubscription = await _messagingProvider.SubscribeAsync(_testTopic, _testConsumerGroup, async (msg, token) =>
            {
                Assert.Fail("First subscription should not receive the message.");
            }, _cts.Token);
            await Task.Delay(500);

            await _messagingProvider.UnsubscribeAsync(firstSubscription, _cts.Token);
            await Task.Delay(500);

            // Act
            await _messagingProvider.PublishAsync(_testTopic, messageToSend, _cts.Token);

            await Task.Delay(2000);

            // Late subscriber
            await _messagingProvider.SubscribeAsync(_testTopic, _testConsumerGroup, async (msg, token) =>
            {
                receivedMessages.Add(msg);
                lateSubscriberReceived.TrySetResult(true);
            }, _cts.Token);

            await Task.Delay(1000);

            // Assert
            try
            {
                await lateSubscriberReceived.Task.WaitAsync(TimeSpan.FromSeconds(5));
            }
            catch (TimeoutException)
            {
                Assert.Fail($"Late subscriber did not receive the expected message '{messageToSend}' within 5 seconds.");
            }

            Assert.Contains(messageToSend, receivedMessages);
        }
        
        [Fact]
        public async Task AtLeastOnceGurantee_ShouldNotDeliverToLateSubscriber_WhenNotSubscribedBefore()
        {
            // Arrange
            var messageToSend = "Non-Persistent Message Test";
            var receivedMessages = new ConcurrentBag<string>();

            // Act
            await _messagingProvider.PublishAsync(_testTopic, messageToSend, _cts.Token);
            await Task.Delay(2000);

            // Late subscriber
            await _messagingProvider.SubscribeAsync(_testTopic, _testConsumerGroup, async (msg, token) =>
            {
                receivedMessages.Add(msg);
            }, _cts.Token);

            await Task.Delay(1000);

            // Assert
            Assert.DoesNotContain(messageToSend, receivedMessages);
        }
        
        [Fact]
        public async Task AtLeastOnceGurantee_ShouldRedeliverLater_WhenMessageNotAcked()
        {
            // Arrange
            var messageToSend = "Redelivery Test";
            var receivedMessages = new ConcurrentBag<string>();

            async Task FaultyHandler(string msg, CancellationToken token)
            {
                receivedMessages.Add(msg);
                throw new Exception("Simulated consumer crash before acknowledgment.");
            }

            await _messagingProvider.SubscribeAsync(_testTopic, _testConsumerGroup, FaultyHandler, _cts.Token);
            await Task.Delay(500);

            // Act
            await _messagingProvider.PublishAsync(_testTopic, messageToSend, _cts.Token);

            for (int i = 0; i < 20; i++)
            {
                if (receivedMessages.Count > 1) break;
                await Task.Delay(500);
            }

            // Assert
            Assert.True(receivedMessages.Count > 1, "Message should be redelivered at least once.");
        }
        
        [Fact]
        public async Task CompetingConsumers_ShouldDeliverOnlyOnce_WhenMultipleConsumersInGroup()
        {
            // Arrange
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

            await _messagingProvider.SubscribeAsync(_testTopic, _testConsumerGroup, MessageHandler, _cts.Token);
            await _messagingProvider.SubscribeAsync(_testTopic, _testConsumerGroup, MessageHandler, _cts.Token);
            await _messagingProvider.SubscribeAsync(_testTopic, _testConsumerGroup, MessageHandler, _cts.Token);
            await Task.Delay(500);

            // Act
            await _messagingProvider.PublishAsync(_testTopic, messageToSend, _cts.Token);

            await Task.Delay(3000);
            
            try
            {
                await allSubscribersProcessed.Task.WaitAsync(TimeSpan.FromSeconds(5));
            }
            catch (TimeoutException)
            {
                Assert.Fail("Timed out waiting for message processing.");
            }

            // Assert
            Assert.Equal(1, receivedMessages.GetValueOrDefault("received", 0));
        }
        
        [Fact]
        public async Task CompetingConsumers_ShouldDistributeMessages_WhenMultipleConsumersInGroup()
        {
            // Arrange
            var receivedMessages = new ConcurrentDictionary<string, int>();
            var totalMessages = 100;
            var numConsumers = 10;
            var variancePercentage = 0.1;
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

                await _messagingProvider.SubscribeAsync(_testTopic, _testConsumerGroup, MessageHandler, _cts.Token);
            }
            
            for (int i = 0; i < numConsumers; i++)
            {
                await CreateConsumer();
            }

            // Act
            for (int i = 0; i < totalMessages; i++)
            {
                await _messagingProvider.PublishAsync(_testTopic, "Test Message", _cts.Token);
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
        
        private async Task EnsureConnectedAsync(CancellationToken cancellationToken)
        {
            if (_connection is { IsOpen: true } && _channel is { IsOpen: true }) return;

            _connection = await _connectionFactory.CreateConnectionAsync(cancellationToken);
            _channel = await _connection.CreateChannelAsync(null, cancellationToken);
        }
        
        private async Task<bool> ExchangeExistsAsync(string exchange, CancellationToken cancellationToken)
        {
            try
            {
                // Use a temporary channel to check exchange existence, since old one might have stale topic data
                using var tempChannel = await _connection!.CreateChannelAsync(null, cancellationToken);
                await tempChannel.ExchangeDeclarePassiveAsync(exchange, cancellationToken);

                return true;
            }
            catch (OperationInterruptedException ex) when (ex.ShutdownReason?.ReplyCode == 404)
            {
                _logger.LogDebug("Exchange '{Exchange}' does not exist yet.", exchange);
                
                await EnsureConnectedAsync(cancellationToken);
                
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while checking if exchange '{Exchange}' exists.");
                throw;
            }
        }
    }
}
