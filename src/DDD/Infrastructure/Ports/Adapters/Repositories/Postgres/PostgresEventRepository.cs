using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DDD.Domain;
using DDD.Infrastructure.Persistence;
using DDD.Infrastructure.Ports.Adapters.Postgres.Exceptions;

namespace DDD.Infrastructure.Ports.Adapters.Repositories.Postgres
{
    public class PostgresEventRepository : IEventRepository
    {
        private readonly IPersistenceService _persistenceService;
        
        public PostgresEventRepository(IPersistenceService persistenceService)
        {
            _persistenceService = persistenceService;
            StartAsync().Wait();
        }
		
        public async Task StartAsync()
        {
            await AssertTables();
        }

        public Task StopAsync()
        {
            return Task.CompletedTask;
        }

        private async Task AssertTables()
        {
            var stmt =
                $"CREATE TABLE IF NOT EXISTS outbox " +
                $"(id VARCHAR UNIQUE NOT NULL," +
                $"data json NOT NULL)";

            using (var conn = await _persistenceService.OpenConnectionAsync())
                await conn.ExecuteNonQueryAsync(stmt);
        }
        
        public async Task SaveAllAsync(ActionId actionId, IEnumerable<IEvent> events, CancellationToken ct)
        {
            if (events.Count() == 0)
                return;
            
            var conn = await _persistenceService.GetConnectionAsync(actionId);

            var values = new List<string>();
            var parameters = new Dictionary<string, object>();

            var count = 0;
            foreach (var theEvent in events)
            {
                values.Add($"(@id_{count}, @data_{count})");
                parameters.Add($"@id_{count}", theEvent.EventId.ToString());
                parameters.Add($"@data_{count}", JsonSerializer.SerializeToDocument(new OutboxEvent(theEvent)));
                count++;
            }

            var stmt =
                $"INSERT INTO outbox (id, data) " +
                $"VALUES {string.Join(",", values)}";
            
            await conn.ExecuteNonQueryAsync(stmt, parameters);
        }

        public async Task<IEnumerable<OutboxEvent>> GetAllAsync(ActionId actionId, CancellationToken ct)
        {
            var conn = await _persistenceService.GetConnectionAsync(actionId);
            var stmt = $"SELECT * FROM outbox WHERE data->>'actionId' = @action_id";
            var parameters = new Dictionary<string, object>();
            parameters.Add("@action_id", actionId.ToString());
            var outboxEvents = await conn.ExecuteQueryAsync<OutboxEvent>(stmt, parameters);
            return outboxEvents;
        }

        public async Task DeleteAsync(EventId eventId, ActionId actionId, CancellationToken ct)
        {
            var conn = await _persistenceService.GetConnectionAsync(actionId);
            var stmt = $"DELETE FROM outbox WHERE id = @id";
            var parameters = new Dictionary<string, object>();
            parameters.Add("@id", eventId.ToString());
            var rows = await conn.ExecuteNonQueryAsync(stmt, parameters);
            if (rows != 1)
                throw new PostgresException($"Couldn't delete aggregate, none found with ID '{eventId}'.");
        }

        public async Task DeleteAllAsync(CancellationToken ct)
        {
            var stmt = $"DELETE FROM outbox";
            using (var conn = await _persistenceService.OpenConnectionAsync())
                await conn.ExecuteNonQueryAsync(stmt);
        }
    }
}
