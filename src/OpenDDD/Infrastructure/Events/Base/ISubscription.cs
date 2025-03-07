namespace OpenDDD.Infrastructure.Events.Base
{
    public interface ISubscription : IAsyncDisposable
    {
        string Id { get; }
        string Topic { get; }
        string ConsumerGroup { get; }
    }
}
