using RabbitMQ.Client;

namespace OpenDDD.Infrastructure.Events.RabbitMq.Factories
{
    public interface IRabbitMqConsumerFactory
    {
        RabbitMqCustomAsyncConsumer CreateConsumer(IChannel channel, Func<string, CancellationToken, Task> messageHandler);
    }
}
