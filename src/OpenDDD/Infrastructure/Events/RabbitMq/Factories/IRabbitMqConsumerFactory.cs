using RabbitMQ.Client;

namespace OpenDDD.Infrastructure.Events.RabbitMq.Factories
{
    public interface IRabbitMqConsumerFactory
    {
        RabbitMqConsumer CreateConsumer(IChannel channel, Func<string, CancellationToken, Task> messageHandler);
    }
}
