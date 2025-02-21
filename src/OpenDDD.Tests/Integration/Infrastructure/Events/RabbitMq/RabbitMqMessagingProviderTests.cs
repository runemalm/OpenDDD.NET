using Microsoft.Extensions.Logging;
using Moq;
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
        private readonly Mock<ILogger<RabbitMqMessagingProvider>> _loggerMock;
        private IConnection? _connection;
        private IChannel? _channel;
        
        private readonly string _testTopic = "OpenDddTestTopic";
        private readonly string _testConsumerGroup = "OpenDddTestGroup";

        public RabbitMqMessagingProviderTests()
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
            await DeleteExchangesAndQueuesAsync();
        }

        public async Task DisposeAsync()
        {
            await DeleteExchangesAndQueuesAsync();

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

        private async Task DeleteExchangesAndQueuesAsync()
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
        public async Task SubscribeAsync_ShouldCreateTopicIfNotExists()
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
    }
}
