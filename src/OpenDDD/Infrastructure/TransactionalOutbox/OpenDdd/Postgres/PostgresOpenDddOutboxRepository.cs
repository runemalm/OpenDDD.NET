using Microsoft.Extensions.Logging;
using OpenDDD.Domain.Model;
using OpenDDD.Infrastructure.Events;
using OpenDDD.Infrastructure.Persistence.DatabaseSession;
using OpenDDD.Infrastructure.Persistence.OpenDdd.DatabaseSession.Postgres;
using Npgsql;

namespace OpenDDD.Infrastructure.TransactionalOutbox.OpenDdd.Postgres
{
    public class PostgresOpenDddOutboxRepository : IOutboxRepository
    {
        private readonly PostgresDatabaseSession _session;
        private readonly ILogger<PostgresOpenDddOutboxRepository> _logger;

        public PostgresOpenDddOutboxRepository(IDatabaseSession session, ILogger<PostgresOpenDddOutboxRepository> logger)
        {
            if (session is not PostgresDatabaseSession postgresSession)
                throw new ArgumentException("Expected PostgresDatabaseSession", nameof(session));

            _session = postgresSession;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task SaveEventAsync<TEvent>(TEvent @event, CancellationToken ct) where TEvent : IEvent
        {
            await _session.OpenConnectionAsync(ct);

            var serializedPayload = EventSerializer.Serialize(@event, @event.GetType());
            var eventType = @event is IIntegrationEvent ? "Integration" : "Domain";
            var eventName = @event.GetType().Name.Replace("IntegrationEvent", "");

            const string query = @"
                INSERT INTO outbox_entries (id, event_type, event_name, payload, created_at, processed_at) 
                VALUES (@id, @event_type, @event_name, @payload, @created_at, NULL);";

            await using var cmd = new NpgsqlCommand(query, _session.Connection, _session.Transaction);
            cmd.Parameters.AddWithValue("id", Guid.NewGuid());
            cmd.Parameters.AddWithValue("event_type", eventType);
            cmd.Parameters.AddWithValue("event_name", eventName);
            cmd.Parameters.Add("payload", NpgsqlTypes.NpgsqlDbType.Jsonb).Value = serializedPayload;
            cmd.Parameters.AddWithValue("created_at", DateTime.UtcNow);

            await cmd.ExecuteNonQueryAsync(ct);
        }

        public async Task<List<OutboxEntry>> GetPendingEventsAsync(int? maxCount = null, CancellationToken ct = default)
        {
            await _session.OpenConnectionAsync(ct);
            
            var now = DateTime.UtcNow;
            var lockUntil = now.AddMinutes(1);
            
            var query = $@"
                UPDATE outbox_entries
                SET locked_until = @lock_until
                WHERE id IN (
                    SELECT id
                    FROM outbox_entries
                    WHERE processed_at IS NULL
                      AND (locked_until IS NULL OR locked_until < @now)
                    ORDER BY created_at
                    {(maxCount.HasValue ? "LIMIT @maxCount" : "")}
                    FOR UPDATE SKIP LOCKED
                )
                RETURNING id, event_type, event_name, payload, created_at, processed_at, locked_until;";

            await using var cmd = new NpgsqlCommand(query, _session.Connection, _session.Transaction);
            cmd.Parameters.AddWithValue("now", now);
            cmd.Parameters.AddWithValue("lock_until", lockUntil);
            if (maxCount.HasValue)
                cmd.Parameters.AddWithValue("maxCount", maxCount.Value);

            var events = new List<OutboxEntry>();

            await using var reader = await cmd.ExecuteReaderAsync(ct);
            while (await reader.ReadAsync(ct))
            {
                events.Add(new OutboxEntry
                {
                    Id = reader.GetGuid(0),
                    EventType = reader.GetString(1),
                    EventName = reader.GetString(2),
                    Payload = reader.GetString(3),
                    CreatedAt = reader.GetDateTime(4),
                    ProcessedAt = reader.IsDBNull(5) ? null : reader.GetDateTime(5),
                    LockedUntil = reader.GetDateTime(6),
                });
            }

            return events;
        }

        public async Task MarkEventAsProcessedAsync(Guid eventId, CancellationToken ct)
        {
            await _session.OpenConnectionAsync(ct);

            const string query = "UPDATE outbox_entries SET processed_at = @processed_at WHERE id = @id;";

            await using var cmd = new NpgsqlCommand(query, _session.Connection, _session.Transaction);
            cmd.Parameters.AddWithValue("id", eventId);
            cmd.Parameters.AddWithValue("processed_at", DateTime.UtcNow);

            await cmd.ExecuteNonQueryAsync(ct);
        }
    }
}
