namespace OpenDDD.Domain.Model
{
    public interface IDomainPublisher
    {
        Task PublishAsync<TEvent>(TEvent domainEvent, CancellationToken ct) where TEvent : IDomainEvent;
        IReadOnlyList<IDomainEvent> GetPublishedEvents();
    }
}
