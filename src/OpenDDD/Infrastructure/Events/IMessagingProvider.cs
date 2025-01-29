namespace OpenDDD.Infrastructure.Events
{
    public interface IMessagingProvider
    {
        Task SubscribeAsync(string topic, string consumerGroup, Func<string, CancellationToken, Task> messageHandler, CancellationToken cancellationToken = default);
        Task PublishAsync(string topic, string message, CancellationToken cancellationToken = default);
    }
}
