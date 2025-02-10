using Microsoft.EntityFrameworkCore;
using OpenDDD.Domain.Model;
using OpenDDD.Infrastructure.Events;
using OpenDDD.Infrastructure.Persistence.DatabaseSession;
using OpenDDD.Infrastructure.Persistence.EfCore.DatabaseSession;

namespace OpenDDD.Infrastructure.TransactionalOutbox.EfCore
{
    public class EfCoreOutboxRepository : IOutboxRepository
    {
        private readonly EfCoreDatabaseSession _session;

        public EfCoreOutboxRepository(IDatabaseSession session)
        {
            if (session is not EfCoreDatabaseSession efCoreSession)
                throw new ArgumentException("Expected EfCoreDatabaseSession", nameof(session));

            _session = efCoreSession;
        }

        public async Task SaveEventAsync<TEvent>(TEvent @event, CancellationToken ct) where TEvent : IEvent
        {
            await _session.OpenConnectionAsync(ct);

            var serializedPayload = EventSerializer.Serialize(@event, @event.GetType());
            var eventType = @event is IIntegrationEvent ? "Integration" : "Domain";
            var eventName = @event.GetType().Name.Replace("IntegrationEvent", "");

            var outboxEntry = new OutboxEntry
            {
                Id = Guid.NewGuid(),
                EventType = eventType,
                EventName = eventName,
                Payload = serializedPayload,
                CreatedAt = DateTime.UtcNow,
                ProcessedAt = null
            };

            await _session.DbContext.Set<OutboxEntry>().AddAsync(outboxEntry, ct);
            await _session.DbContext.SaveChangesAsync(ct);
        }

        public async Task<List<OutboxEntry>> GetPendingEventsAsync(CancellationToken ct)
        {
            await _session.OpenConnectionAsync(ct);

            return await _session.DbContext.Set<OutboxEntry>()
                .Where(e => e.ProcessedAt == null)
                .OrderBy(e => e.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task MarkEventAsProcessedAsync(Guid eventId, CancellationToken ct)
        {
            await _session.OpenConnectionAsync(ct);

            var entry = await _session.DbContext.Set<OutboxEntry>().FindAsync(new object[] { eventId }, ct);

            if (entry == null) return;

            entry.ProcessedAt = DateTime.UtcNow;
            await _session.DbContext.SaveChangesAsync(ct);
        }
    }
}
