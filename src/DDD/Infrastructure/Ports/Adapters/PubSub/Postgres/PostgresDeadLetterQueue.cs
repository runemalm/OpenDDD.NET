using System.Collections.Generic;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using DDD.Infrastructure.Ports.PubSub;
using DDD.Infrastructure.Services.Persistence;

namespace DDD.Infrastructure.Ports.Adapters.PubSub.Postgres
{
    public class PostgresDeadLetterQueue : IDeadLetterQueue
    {
        private readonly IPersistenceService _persistenceService;
        
        public PostgresDeadLetterQueue(IPersistenceService persistenceService)
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
                $"CREATE TABLE IF NOT EXISTS dead_letter_queue " +
                $"(id VARCHAR UNIQUE NOT NULL," +
                $"data json NOT NULL)";

            using (var conn = await _persistenceService.OpenConnectionAsync())
                await conn.ExecuteNonQueryAsync(stmt);
        }
        
        public async Task EnqueueAsync(DeadEvent deadEvent, CancellationToken ct)
        {
            using (var conn = await _persistenceService.OpenConnectionAsync())
            {
                var stmt =
                    $"INSERT INTO dead_letter_queue (id, data) VALUES (@id, @data)";
                var parameters = new Dictionary<string, object>();
                parameters.Add("@id", deadEvent.Id);
                parameters.Add("@data", JsonSerializer.SerializeToDocument(deadEvent));
                await conn.ExecuteNonQueryAsync(stmt, parameters);
            }
        }

        public async Task EmptyAsync(CancellationToken ct)
        {
            using (var conn = await _persistenceService.OpenConnectionAsync())
            {
                var stmt = $"DELETE FROM dead_letter_queue";
                await conn.ExecuteNonQueryAsync(stmt);
            }
        }
    }
}
