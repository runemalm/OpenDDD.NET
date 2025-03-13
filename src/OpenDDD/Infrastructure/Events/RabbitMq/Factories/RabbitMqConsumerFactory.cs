using Microsoft.Extensions.Logging;
using RabbitMQ.Client;

namespace OpenDDD.Infrastructure.Events.RabbitMq.Factories
{
    public class RabbitMqConsumerFactory : IRabbitMqConsumerFactory
    {
        private readonly ILogger<RabbitMqMessagingProvider> _logger;

        public RabbitMqConsumerFactory(ILogger<RabbitMqMessagingProvider> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public RabbitMqConsumer CreateConsumer(IChannel channel, Func<string, CancellationToken, Task> messageHandler)
        {
            if (channel == null) throw new ArgumentNullException(nameof(channel));
            if (messageHandler == null) throw new ArgumentNullException(nameof(messageHandler));

            return new RabbitMqConsumer(channel, messageHandler, _logger);
        }
    }
}
