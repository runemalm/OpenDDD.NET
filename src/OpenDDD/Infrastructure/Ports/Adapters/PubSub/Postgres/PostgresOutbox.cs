﻿using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using OpenDDD.Application;
using OpenDDD.Domain.Model.BuildingBlocks.Event;
using OpenDDD.Infrastructure.Ports.Adapters.Common.Exceptions;
using OpenDDD.Infrastructure.Ports.Adapters.Common.Translation.Converters;
using OpenDDD.Infrastructure.Ports.PubSub;
using OpenDDD.Infrastructure.Services.Persistence;

namespace OpenDDD.Infrastructure.Ports.Adapters.PubSub.Postgres
{
    public class PostgresOutbox : IOutbox
    {
        private readonly IPersistenceService _persistenceService;
        private readonly SerializerSettings _serializerSettings;
        
        public PostgresOutbox(IPersistenceService persistenceService, SerializerSettings serializerSettings)
        {
            _persistenceService = persistenceService;
            _serializerSettings = serializerSettings;
        }
		
        public void Start(CancellationToken ct)
        {
            AssertTables();
        }

        public async Task StartAsync(CancellationToken ct)
        {
            await AssertTablesAsync();
        }

        public void Stop(CancellationToken ct)
        {
			
        }

        public Task StopAsync(CancellationToken ct)
        {
            return Task.CompletedTask;
        }

        private void AssertTables()
        {
            var stmt = BuildAssertTablesQuery();
            using var conn = _persistenceService.OpenConnection();
            conn.ExecuteNonQuery(stmt);
        }

        private async Task AssertTablesAsync()
        {
            var stmt = BuildAssertTablesQuery();
            using var conn = await _persistenceService.OpenConnectionAsync();
            await conn.ExecuteNonQueryAsync(stmt);
        }

        private string BuildAssertTablesQuery()
        {
            var stmt =
                "CREATE TABLE IF NOT EXISTS outbox " +
                "(id VARCHAR UNIQUE NOT NULL, data jsonb NOT NULL)";
            return stmt;
        }

        public async Task AddAsync(ActionId actionId, IEvent theEvent, CancellationToken ct)
            => AddAllAsync(actionId, new List<IEvent>{theEvent}, ct);
        
        public async Task AddAllAsync(ActionId actionId, IEnumerable<IEvent> events, CancellationToken ct)
        {
            if (events.Count() == 0)
                return;
            
            var conn = await _persistenceService.GetConnectionAsync(actionId);

            var values = new List<string>();
            var parameters = new Dictionary<string, object>();

            var count = 0;
            foreach (var theEvent in events)
            {
                var outboxEvent = new OutboxEvent(theEvent, _serializerSettings);
                values.Add($"(@id_{count}, @data_{count})");
                parameters.Add($"@id_{count}", outboxEvent.Id);
                parameters.Add($"@data_{count}", JsonDocument.Parse(JsonConvert.SerializeObject(outboxEvent, _serializerSettings)));
                count++;
            }

            var stmt =
                $"INSERT INTO outbox (id, data) " +
                $"VALUES {string.Join(",", values)}";
            
            await conn.ExecuteNonQueryAsync(stmt, parameters);
        }
        
        public async Task<OutboxEvent?> GetNextAsync(CancellationToken ct)
        {
            OutboxEvent? outboxEvent = null;
            using (var conn = await _persistenceService.OpenConnectionAsync())
            {
                var stmt = 
                    $"UPDATE outbox " +
                        $"SET data = jsonb_set(data, '{{isPublishing}}', 'true'::jsonb) " +
                    $"WHERE id = ( " +
                        $"SELECT id from outbox " +
                        $"WHERE data->>'isPublishing' = 'false'" +
                        $"ORDER BY data->>'addedAt' ASC " +
                        $"LIMIT 1" +
                    $") " +
                    $"RETURNING *;";

                var outboxEvents = await conn.ExecuteQueryAsync<OutboxEvent>(stmt, null);
                
                if (outboxEvents.Count() == 1)
                    outboxEvent = outboxEvents.First();
            }
            return outboxEvent;
        }
        
        public async Task MarkAsNotPublishingAsync(string id, CancellationToken ct)
        {
            using (var conn = await _persistenceService.OpenConnectionAsync())
            {
                var stmt = 
                    $"UPDATE outbox " +
                    $"SET data = jsonb_set(data, '{{isPublishing}}', 'false'::jsonb) " +
                    $"WHERE id = @id";
                var parameters = new Dictionary<string, object>();
                parameters.Add("@id", id);
                var rows = await conn.ExecuteNonQueryAsync(stmt, parameters);
                if (rows != 1)
                    throw new PostgresException($"Couldn't return outbox event, none found with outbox event ID '{id}'.");
            }
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

        public async Task RemoveAsync(string id, ActionId actionId, CancellationToken ct)
        {
            var conn = await _persistenceService.GetConnectionAsync(actionId);
            var stmt = $"DELETE FROM outbox WHERE Id = @id";
            var parameters = new Dictionary<string, object>();
            parameters.Add("@id", id);
            var rows = await conn.ExecuteNonQueryAsync(stmt, parameters);
            if (rows != 1)
                throw new PostgresException($"Couldn't delete outbox event, none found with outbox ID '{id}'.");
        }
        
        public async Task RemoveAsync(string id, CancellationToken ct)
        {
            using (var conn = await _persistenceService.OpenConnectionAsync())
            {
                var stmt = $"DELETE FROM outbox WHERE Id = @id";
                var parameters = new Dictionary<string, object>();
                parameters.Add("@id", id);
                var rows = await conn.ExecuteNonQueryAsync(stmt, parameters);
                if (rows != 1)
                    throw new PostgresException($"Couldn't delete outbox event, none found with outbox ID '{id}'.");
            }
        }

        public void Empty(CancellationToken ct)
        {
            using var conn = _persistenceService.OpenConnection();
            var stmt = BuildEmptyQuery();
            conn.ExecuteNonQuery(stmt);
        }

        public async Task EmptyAsync(CancellationToken ct)
        {
            using var conn = await _persistenceService.OpenConnectionAsync();
            var stmt = BuildEmptyQuery();
            await conn.ExecuteNonQueryAsync(stmt);
        }

        private string BuildEmptyQuery()
        {
            var stmt = $"DELETE FROM outbox";
            return stmt;
        }
    }
}