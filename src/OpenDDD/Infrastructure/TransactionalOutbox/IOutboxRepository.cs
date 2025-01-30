using OpenDDD.Domain.Model;

namespace OpenDDD.Infrastructure.TransactionalOutbox
{
    public interface IOutboxRepository
    {
        Task SaveEventAsync<TEvent>(TEvent @event, CancellationToken ct) where TEvent : IEvent;
        Task<List<OutboxEntry>> GetPendingEventsAsync(CancellationToken ct);
        Task MarkEventAsProcessedAsync(Guid eventId, CancellationToken ct);
    }
}
