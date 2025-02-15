using Microsoft.Extensions.Logging;
using OpenDDD.Domain.Model;
using OpenDDD.Infrastructure.Events;
using OpenDDD.Infrastructure.Persistence.OpenDdd.DatabaseSession.InMemory;

namespace OpenDDD.Infrastructure.TransactionalOutbox.OpenDdd.InMemory
{
    public class InMemoryOpenDddOutboxRepository : IOutboxRepository
    {
        private readonly InMemoryDatabaseSession _session;
        private readonly ILogger<InMemoryOpenDddOutboxRepository> _logger;
        private const string OutboxTable = "outbox_entries";

        public InMemoryOpenDddOutboxRepository(
            InMemoryDatabaseSession session,
            ILogger<InMemoryOpenDddOutboxRepository> logger)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task SaveEventAsync<TEvent>(TEvent @event, CancellationToken ct) where TEvent : IEvent
        {
            if (@event == null) throw new ArgumentNullException(nameof(@event));
            
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

            await _session.SaveAsync(OutboxTable, outboxEntry.Id, outboxEntry, ct);

            _logger.LogDebug("Added event to in-memory outbox: {EventName}", outboxEntry.EventName);
        }

        public async Task<List<OutboxEntry>> GetPendingEventsAsync(CancellationToken ct)
        {
            var entries = await _session.LoadAllAsync<OutboxEntry>(OutboxTable, ct);
            return entries.Where(entry => entry.ProcessedAt == null).ToList();
        }

        public async Task MarkEventAsProcessedAsync(Guid eventId, CancellationToken ct)
        {
            var entry = await _session.LoadAsync<OutboxEntry>(OutboxTable, eventId, ct);
            if (entry != null)
            {
                entry.ProcessedAt = DateTime.UtcNow;
                await _session.SaveAsync(OutboxTable, eventId, entry, ct);
                _logger.LogDebug("Marked event as processed: {EventId}", eventId);
            }
        }
    }
}
