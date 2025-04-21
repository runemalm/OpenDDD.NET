using OpenDDD.Domain.Model;

namespace OpenDDD.Infrastructure.TransactionalOutbox
{
    public interface IOutboxRepository
    {
        Task SaveEventAsync<TEvent>(TEvent @event, CancellationToken ct) where TEvent : IEvent;
        Task<List<OutboxEntry>> GetPendingEventsAsync(int? maxCount = null, CancellationToken ct = default);
        Task MarkEventAsProcessedAsync(Guid eventId, CancellationToken ct);
    }
}
