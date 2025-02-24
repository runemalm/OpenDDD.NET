using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Moq;
using OpenDDD.Infrastructure.Events.RabbitMq;
using OpenDDD.Infrastructure.Events.RabbitMq.Factories;
using OpenDDD.Tests.Base;
using RabbitMQ.Client;
using RabbitMQ.Client.Exceptions;
using Xunit.Abstractions;

namespace OpenDDD.Tests.Integration.Infrastructure.Events.RabbitMq
{
    [Collection("RabbitMqTests")]
    public class RabbitMqMessagingProviderTests : IntegrationTests, IAsyncLifetime
    {
        private readonly RabbitMqMessagingProvider _messagingProvider;
        private readonly IConnectionFactory _connectionFactory;
        private readonly IRabbitMqConsumerFactory _consumerFactory;
        private readonly Mock<ILogger<RabbitMqMessagingProvider>> _loggerMock;
        private IConnection? _connection;
        private IChannel? _channel;
        
        private readonly string _testTopic = "OpenDddTestTopic";
        private readonly string _testConsumerGroup = "OpenDddTestGroup";

        public RabbitMqMessagingProviderTests(ITestOutputHelper testOutputHelper) : base(testOutputHelper)
        {
            _loggerMock = new Mock<ILogger<RabbitMqMessagingProvider>>();

            _connectionFactory = new ConnectionFactory
            {
                HostName = Environment.GetEnvironmentVariable("RABBITMQ_HOST") ?? "localhost",
                Port = int.Parse(Environment.GetEnvironmentVariable("RABBITMQ_PORT") ?? "5672"),
                UserName = Environment.GetEnvironmentVariable("RABBITMQ_USERNAME") ?? "guest",
                Password = Environment.GetEnvironmentVariable("RABBITMQ_PASSWORD") ?? "guest",
                VirtualHost = Environment.GetEnvironmentVariable("RABBITMQ_VHOST") ?? "/"
            };

            _consumerFactory = new RabbitMqConsumerFactory(_loggerMock.Object);
            _messagingProvider = new RabbitMqMessagingProvider(_connectionFactory, _consumerFactory, _loggerMock.Object);
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
            await VerifyExchangeAndQueueDoNotExist();

            // Act
            await _messagingProvider.SubscribeAsync(_testTopic, _testConsumerGroup, (msg, token) => Task.CompletedTask);

            // Assert
            try
            {
                await _channel!.ExchangeDeclarePassiveAsync(_testTopic, CancellationToken.None);
            }
            catch (OperationInterruptedException)
            {
                Assert.Fail($"Exchange '{_testTopic}' does not exist.");
            }

            try
            {
                await _channel!.QueueDeclarePassiveAsync($"{_testConsumerGroup}.{_testTopic}", CancellationToken.None);
            }
            catch (OperationInterruptedException)
            {
                Assert.Fail($"Queue '{_testConsumerGroup}.{_testTopic}' does not exist.");
            }
        }
        
        [Fact]
        public async Task AtLeastOnceGurantee_ShouldDeliverToLateSubscriber_WhenSubscribedBefore()
        {
            // Arrange
            var receivedMessages = new ConcurrentBag<string>();
            var messageToSend = "Persistent Message Test";
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            // First subscription to establish the listener group
            await _messagingProvider.SubscribeAsync(_testTopic, _testConsumerGroup, async (msg, token) =>
            {
                Assert.Fail("First subscription should not receive the message.");
            }, cts.Token);
            await Task.Delay(500);
            
            // Unsubscribe
            await _messagingProvider.UnsubscribeAsync(_testTopic, _testConsumerGroup, cts.Token);
            await Task.Delay(500);

            // Act: Publish message
            await _messagingProvider.PublishAsync(_testTopic, messageToSend, cts.Token);

            // Delay to simulate late subscriber
            await Task.Delay(2000);

            // Late subscriber
            await _messagingProvider.SubscribeAsync(_testTopic, _testConsumerGroup, async (msg, token) =>
            {
                receivedMessages.Add(msg);
            }, cts.Token);

            // Wait for message processing
            await Task.Delay(1000);

            // Assert
            Assert.Contains(messageToSend, receivedMessages);
        }
        
        [Fact]
        public async Task AtLeastOnceGurantee_ShouldNotDeliverToLateSubscriber_WhenNotSubscribedBefore()
        {
            // Arrange
            var receivedMessages = new ConcurrentBag<string>();
            var messageToSend = "Non-Persistent Message Test";
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            // Act: Publish message before any subscription
            await _messagingProvider.PublishAsync(_testTopic, messageToSend, cts.Token);

            // Delay to simulate late subscriber
            await Task.Delay(2000);

            // Late subscriber
            await _messagingProvider.SubscribeAsync(_testTopic, _testConsumerGroup, async (msg, token) =>
            {
                receivedMessages.Add(msg);
            }, cts.Token);

            // Wait for message processing
            await Task.Delay(1000);

            // Assert
            Assert.DoesNotContain(messageToSend, receivedMessages);
        }
        
        [Fact]
        public async Task AtLeastOnceGurantee_ShouldRedeliverLater_WhenMessageNotAcked()
        {
            // Arrange
            var receivedMessages = new ConcurrentBag<string>();
            var messageToSend = "Redelivery Test";
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            async Task FaultyHandler(string msg, CancellationToken token)
            {
                receivedMessages.Add(msg);
                throw new Exception("Simulated consumer crash before acknowledgment.");
            }

            await _messagingProvider.SubscribeAsync(_testTopic, _testConsumerGroup, FaultyHandler, cts.Token);
            await Task.Delay(500); // Ensure setup

            // Act: Publish message
            await _messagingProvider.PublishAsync(_testTopic, messageToSend, cts.Token);

            // Wait for redelivery
            await Task.Delay(3000);

            // Assert: The message should be received multiple times due to reattempts
            Assert.True(receivedMessages.Count > 1, "Message should be redelivered at least once.");
        }
        
        [Fact]
        public async Task CompetingConsumers_ShouldDeliverOnlyOnce_WhenMultipleConsumersInGroup()
        {
            // Arrange
            var receivedMessages = new ConcurrentDictionary<string, int>();
            var messageToSend = "Competing Consumer Test";
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10));

            async Task MessageHandler(string msg, CancellationToken token)
            {
                receivedMessages.AddOrUpdate("received", 1, (key, value) => value + 1);
            }

            // Multiple competing consumers in the same group
            await _messagingProvider.SubscribeAsync(_testTopic, _testConsumerGroup, MessageHandler, cts.Token);
            await _messagingProvider.SubscribeAsync(_testTopic, _testConsumerGroup, MessageHandler, cts.Token);
            await _messagingProvider.SubscribeAsync(_testTopic, _testConsumerGroup, MessageHandler, cts.Token);
            await Task.Delay(500); // Allow time for consumers to start

            // Act
            await _messagingProvider.PublishAsync(_testTopic, messageToSend, cts.Token);

            // Wait for processing
            await Task.Delay(1000);

            // Assert: Only one of the competing consumers should receive the message
            Assert.Equal(1, receivedMessages.GetValueOrDefault("received", 0));
        }
        
        [Fact]
        public async Task CompetingConsumers_ShouldDistributeEvenly_WhenMultipleConsumersInGroup()
        {
            // Arrange
            var receivedMessages = new ConcurrentDictionary<string, int>();
            var totalMessages = 10;
            var cts = new CancellationTokenSource(TimeSpan.FromSeconds(15));

            async Task MessageHandler(string msg, CancellationToken token)
            {
                receivedMessages.AddOrUpdate(msg, 1, (key, value) => value + 1);
            }

            // Multiple competing consumers in the same group
            await _messagingProvider.SubscribeAsync(_testTopic, _testConsumerGroup, MessageHandler, cts.Token);
            await _messagingProvider.SubscribeAsync(_testTopic, _testConsumerGroup, MessageHandler, cts.Token);
            await _messagingProvider.SubscribeAsync(_testTopic, _testConsumerGroup, MessageHandler, cts.Token);
            await Task.Delay(500); // Allow time for consumers to start

            // Act: Publish multiple messages
            for (int i = 0; i < totalMessages; i++)
            {
                await _messagingProvider.PublishAsync(_testTopic, $"Message {i}", cts.Token);
            }

            // Wait for processing
            await Task.Delay(2000);

            // Assert: Messages should be evenly distributed across consumers
            var messageCounts = receivedMessages.Values;
            var minReceived = messageCounts.Min();
            var maxReceived = messageCounts.Max();
    
            Assert.True(maxReceived - minReceived <= 1, "Messages should be evenly distributed among competing consumers.");
        }
    }
}
