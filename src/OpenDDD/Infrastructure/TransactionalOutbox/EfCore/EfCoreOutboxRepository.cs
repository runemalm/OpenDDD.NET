using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
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
        
        public async Task<List<OutboxEntry>> GetPendingEventsAsync(int? maxCount = null, CancellationToken ct = default)
        {
            await _session.OpenConnectionAsync(ct);

            var lockDuration = TimeSpan.FromMinutes(1);
            var lockUntil = DateTime.UtcNow.Add(lockDuration);

            var table = _session.DbContext.Model.FindEntityType(typeof(OutboxEntry))!.GetTableName();

            var sql = $@"
                UPDATE {table}
                SET locked_until = @lockUntil
                WHERE id IN (
                    SELECT id
                    FROM {table}
                    WHERE processed_at IS NULL
                      AND (locked_until IS NULL OR locked_until < @now)
                    ORDER BY created_at
                    {(maxCount.HasValue ? "LIMIT @maxCount" : "")}
                    FOR UPDATE SKIP LOCKED
                )
                RETURNING id, event_type, event_name, payload, created_at, processed_at, locked_until;";

            var conn = _session.DbContext.Database.GetDbConnection();
            await using var cmd = conn.CreateCommand();
            cmd.CommandText = sql;
            cmd.Transaction = _session.DbContext.Database.CurrentTransaction?.GetDbTransaction();

            var nowParam = cmd.CreateParameter();
            nowParam.ParameterName = "now";
            nowParam.Value = DateTime.UtcNow;
            cmd.Parameters.Add(nowParam);

            var lockUntilParam = cmd.CreateParameter();
            lockUntilParam.ParameterName = "lockUntil";
            lockUntilParam.Value = lockUntil;
            cmd.Parameters.Add(lockUntilParam);

            if (maxCount.HasValue)
            {
                var maxCountParam = cmd.CreateParameter();
                maxCountParam.ParameterName = "maxCount";
                maxCountParam.Value = maxCount.Value;
                cmd.Parameters.Add(maxCountParam);
            }

            await conn.OpenAsync(ct);
            var reader = await cmd.ExecuteReaderAsync(ct);

            var result = new List<OutboxEntry>();
            while (await reader.ReadAsync(ct))
            {
                result.Add(new OutboxEntry
                {
                    Id = reader.GetGuid(0),
                    EventType = reader.GetString(1),
                    EventName = reader.GetString(2),
                    Payload = reader.GetString(3),
                    CreatedAt = reader.GetDateTime(4),
                    ProcessedAt = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
                    LockedUntil = reader.IsDBNull(6) ? null : reader.GetDateTime(6)
                });
            }

            return result;
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
