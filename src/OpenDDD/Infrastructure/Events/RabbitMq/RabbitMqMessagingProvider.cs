using System.Text;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using OpenDDD.API.Options;
using OpenDDD.Infrastructure.Events.RabbitMq.Options;
using RabbitMQ.Client;

namespace OpenDDD.Infrastructure.Events.RabbitMq
{
    public class RabbitMqMessagingProvider : IMessagingProvider, IAsyncDisposable
    {
        private readonly ConnectionFactory _factory;
        private IConnection? _connection;
        private IChannel? _channel;
        private readonly OpenDddRabbitMqOptions _options;
        private readonly ILogger<RabbitMqMessagingProvider> _logger;

        public RabbitMqMessagingProvider(
            IOptions<OpenDddOptions> options,
            ILogger<RabbitMqMessagingProvider> logger)
        {
            var openDddOptions = options?.Value ?? throw new ArgumentNullException(nameof(options));
            _options = openDddOptions.RabbitMq ?? throw new InvalidOperationException("RabbitMQ settings are missing in OpenDddOptions.");

            if (string.IsNullOrWhiteSpace(_options.HostName))
            {
                throw new InvalidOperationException("RabbitMQ host is missing.");
            }

            _factory = new ConnectionFactory
            {
                HostName = _options.HostName,
                Port = _options.Port,
                UserName = _options.Username,
                Password = _options.Password,
                VirtualHost = _options.VirtualHost
            };

            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private async Task EnsureConnectedAsync(CancellationToken cancellationToken)
        {
            if (_connection is { IsOpen: true } && _channel is { IsOpen: true }) return;

            _connection = await _factory.CreateConnectionAsync(cancellationToken);
            _channel = await _connection.CreateChannelAsync(null, cancellationToken);
        }

        public async Task PublishAsync(string topic, string message, CancellationToken cancellationToken = default)
        {
            await EnsureConnectedAsync(cancellationToken);

            if (_channel is null) throw new InvalidOperationException("RabbitMQ channel is not available.");

            await _channel.ExchangeDeclareAsync(topic, ExchangeType.Fanout, durable: true, autoDelete: false, cancellationToken: cancellationToken);

            var body = Encoding.UTF8.GetBytes(message);
            await _channel.BasicPublishAsync(topic, "", body, cancellationToken: cancellationToken);

            _logger.LogInformation("Published message to topic '{Topic}'", topic);
        }

        public async Task SubscribeAsync(string topic, string consumerGroup, Func<string, CancellationToken, Task> messageHandler, CancellationToken cancellationToken = default)
        {
            await EnsureConnectedAsync(cancellationToken);

            if (_channel is null) throw new InvalidOperationException("RabbitMQ channel is not available.");

            await _channel.ExchangeDeclareAsync(topic, ExchangeType.Fanout, durable: true, autoDelete: false, cancellationToken: cancellationToken);
            var queueName = $"{consumerGroup}.{topic}";
            await _channel.QueueDeclareAsync(queueName, durable: true, exclusive: false, autoDelete: false, cancellationToken: cancellationToken);
            await _channel.QueueBindAsync(queueName, topic, "", cancellationToken: cancellationToken);

            var consumer = new RabbitMqCustomAsyncConsumer(_channel, messageHandler, _logger);
            await _channel.BasicConsumeAsync(queueName, autoAck: false, consumer, cancellationToken);

            _logger.LogInformation("Subscribed to RabbitMQ topic '{Topic}' with consumer group '{ConsumerGroup}'", topic, consumerGroup);
        }

        public async ValueTask DisposeAsync()
        {
            if (_channel is not null)
            {
                await _channel.CloseAsync();
                _channel.Dispose();
            }

            if (_connection is not null)
            {
                await _connection.CloseAsync();
                _connection.Dispose();
            }
        }
    }
}
