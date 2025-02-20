using System.Text;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace OpenDDD.Infrastructure.Events.RabbitMq
{
    public class RabbitMqCustomAsyncConsumer : IAsyncBasicConsumer
    {
        private readonly Func<string, CancellationToken, Task> _messageHandler;
        private readonly ILogger<RabbitMqMessagingProvider> _logger;
        private bool _disposed;

        public RabbitMqCustomAsyncConsumer(
            IChannel channel, 
            Func<string, CancellationToken, Task> messageHandler, 
            ILogger<RabbitMqMessagingProvider> logger)
        {
            Channel = channel ?? throw new ArgumentNullException(nameof(channel));
            _messageHandler = messageHandler ?? throw new ArgumentNullException(nameof(messageHandler));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public IChannel? Channel { get; }

        public async Task HandleBasicDeliverAsync(
            string consumerTag,
            ulong deliveryTag,
            bool redelivered,
            string exchange,
            string routingKey,
            IReadOnlyBasicProperties properties,
            ReadOnlyMemory<byte> body,
            CancellationToken cancellationToken = default)
        {
            var message = Encoding.UTF8.GetString(body.ToArray());

            try
            {
                _logger.LogInformation("Received message from RabbitMQ: {Message}", message);
                await _messageHandler(message, cancellationToken);
                await Channel!.BasicAckAsync(deliveryTag, multiple: false, cancellationToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing RabbitMQ message: {Message}", message);
                await Channel!.BasicNackAsync(deliveryTag, multiple: false, requeue: true, cancellationToken);
            }
        }

        public Task HandleBasicConsumeOkAsync(string consumerTag, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Consumer {ConsumerTag} registered successfully.", consumerTag);
            return Task.CompletedTask;
        }

        public Task HandleBasicCancelAsync(string consumerTag, CancellationToken cancellationToken = default)
        {
            _logger.LogWarning("Consumer {ConsumerTag} was cancelled.", consumerTag);
            return Task.CompletedTask;
        }

        public Task HandleBasicCancelOkAsync(string consumerTag, CancellationToken cancellationToken = default)
        {
            _logger.LogInformation("Consumer {ConsumerTag} was successfully deregistered.", consumerTag);
            return Task.CompletedTask;
        }

        public Task HandleChannelShutdownAsync(object channel, ShutdownEventArgs reason)
        {
            _logger.LogWarning("Channel was shut down. Reason: {Reason}", reason.ReplyText);
            return Task.CompletedTask;
        }
    }
}
