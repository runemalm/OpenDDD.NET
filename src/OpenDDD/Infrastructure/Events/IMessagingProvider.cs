using OpenDDD.Infrastructure.Events.Base;

namespace OpenDDD.Infrastructure.Events
{
    public interface IMessagingProvider
    {
        Task<ISubscription> SubscribeAsync(
            string topic,
            string consumerGroup,
            Func<string, CancellationToken, Task> messageHandler,
            CancellationToken cancellationToken = default);

        Task UnsubscribeAsync(ISubscription subscription, CancellationToken cancellationToken = default);
        Task PublishAsync(string topic, string message, CancellationToken cancellationToken = default);
    }
}
