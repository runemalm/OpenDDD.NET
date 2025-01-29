namespace OpenDDD.Domain.Model
{
    public interface IIntegrationPublisher
    {
        Task PublishAsync<TEvent>(TEvent integrationEvent, CancellationToken ct) where TEvent : IIntegrationEvent;
        IReadOnlyList<IIntegrationEvent> GetPublishedEvents();
    }
}
