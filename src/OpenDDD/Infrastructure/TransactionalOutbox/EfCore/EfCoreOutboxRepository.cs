using Microsoft.EntityFrameworkCore;
using OpenDDD.Domain.Model;
using OpenDDD.Infrastructure.Events;
using OpenDDD.Infrastructure.Persistence.EfCore.Base;

namespace OpenDDD.Infrastructure.TransactionalOutbox.EfCore
{
    public class EfCoreOutboxRepository : IOutboxRepository
    {
        private readonly OpenDddDbContextBase _dbContext;

        public EfCoreOutboxRepository(OpenDddDbContextBase dbContext)
        {
            _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public async Task SaveEventAsync<TEvent>(TEvent @event, CancellationToken ct)
            where TEvent : IEvent
        {
            var eventClassType = @event.GetType(); 
    
            var serializedPayload = EventSerializer.Serialize(@event, eventClassType);
    
            // Determine event type and name based on convention
            var eventType = @event is IIntegrationEvent ? "Integration" : "Domain";
            var eventName = @event.GetType().Name.Replace("IntegrationEvent", "");

            var outboxEntry = new OutboxEntry
            {
                Id = Guid.NewGuid(),
                EventType = eventType,
                EventName = eventName,
                Payload = serializedPayload,
                CreatedAt = DateTime.UtcNow,
                Processed = false
            };

            await _dbContext.Set<OutboxEntry>().AddAsync(outboxEntry, ct);
            await _dbContext.SaveChangesAsync(ct);
        }

        public async Task<List<OutboxEntry>> GetPendingEventsAsync(CancellationToken ct)
        {
            return await _dbContext.Set<OutboxEntry>()
                .Where(e => !e.Processed)
                .OrderBy(e => e.CreatedAt)
                .ToListAsync(ct);
        }

        public async Task MarkEventAsProcessedAsync(Guid eventId, CancellationToken ct)
        {
            var entry = await _dbContext.Set<OutboxEntry>().FindAsync(new object[] { eventId }, ct);

            if (entry == null) return;

            entry.Processed = true;
            entry.ProcessedAt = DateTime.UtcNow;

            await _dbContext.SaveChangesAsync(ct);
        }
    }
}
