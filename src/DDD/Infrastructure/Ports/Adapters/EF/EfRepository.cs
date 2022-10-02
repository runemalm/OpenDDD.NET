// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Linq.Expressions;
// using System.Threading;
// using System.Threading.Tasks;
// using System.Data.SqlClient;
// using Npgsql;
// using JsonSerializer = System.Text.Json.JsonSerializer;
// using PostgresException = DDD.Infrastructure.Ports.Adapters.Postgres.Exceptions.PostgresException;
// using DDD.Domain;
// using DDD.Infrastructure.Persistence;
// using DDD.Application.Settings;
//
// namespace DDD.Infrastructure.Ports.Adapters.EF
// {
// 	public abstract class EfRepository<T, TId> : Repository<T>, IRepository<T> where T : IAggregate where TId : EntityId
// 	{
// 		private readonly ISettings _settings;
// 		private readonly string _tableName;
// 		private readonly IMigrator<T> _migrator;
// 		private readonly PersistenceService _persistenceService;
//
// 		public EfRepository(ISettings settings, string tableName, IMigrator<T> migrator, PersistenceService persistenceService)
// 		{
// 			_settings = settings;
// 			_tableName = tableName;
// 			_migrator = migrator;
// 			_persistenceService = persistenceService;
// 		
// 			StartAsync().Wait();
// 		}
// 		
// 		public async Task StartAsync()
// 		{
// 			// await Connect();
// 			await AssertTables();
// 		}
//
// 		public Task StopAsync()
// 		{
// 			// Disconnect();
// 			return Task.CompletedTask;
// 		}
//
// 		private async Task AssertTables()
// 		{
// 			var sql =
// 				$"CREATE TABLE IF NOT EXISTS {_tableName} " +
// 				$"(id VARCHAR UNIQUE NOT NULL,data json NOT NULL)";
// 			
// 			SqlCommand cmd = new SqlCommand(sql);
// 			
// 			// cmd.Parameters.Add("@ID", SqlDbType.Int);
// 			// cmd.Parameters["@ID"].Value = customerID;
// 			// // Use AddWithValue to assign Demographics.
// 			// // SQL Server will implicitly convert strings into XML.
// 			// cmd.Parameters.AddWithValue("@demographics", demoXml);
//
// 			using (var conn = await _persistenceService.OpenConnectionAsync())
// 			{
// 				await conn.ExecuteNonQueryAsync(cmd);				
// 			}
// 		}
//
// 		public override async Task DeleteAllAsync(CancellationToken ct)
// 		{
// 			var sql = $"DELETE FROM {_tableName}";
// 			using (var cmd = new NpgsqlCommand(sql, _conn))
// 			{
// 				await cmd.ExecuteNonQueryAsync(ct);
// 			}
// 		}
// 		
// 		public override async Task DeleteAsync(EntityId entityId, CancellationToken ct)
// 		{
// 			var sql = $"DELETE FROM {_tableName} WHERE id = @id";
// 			using (var cmd = new NpgsqlCommand(sql, _conn))
// 			{
// 				cmd.Parameters.AddWithValue("id", entityId.ToString());
// 				var rows = await cmd.ExecuteNonQueryAsync(ct);
// 				if (rows != 1)
// 					throw new PostgresException($"Couldn't delete aggregate, none found with ID '{entityId}'.");
// 			}
// 		}
//
// 		public override async Task<IEnumerable<T>> GetAllAsync(CancellationToken ct)
// 		{
// 			var aggregates = new List<T>();
// 			var sql = $"SELECT * FROM {_tableName}";
// 			using (var cmd = new NpgsqlCommand(sql, _conn))
// 			await using (var reader = await cmd.ExecuteReaderAsync(ct))
// 			{
// 				while (await reader.ReadAsync(ct))
// 				{
// 					var aggregate = JsonSerializer.Deserialize<T>(reader.GetFieldValue<string>(1));
// 					aggregates.Add(aggregate);
// 				}
// 			}
// 			aggregates = _migrator.Migrate(aggregates).ToList();
// 			return aggregates;
// 		}
// 		
// 		public override async Task<T> GetAsync(EntityId entityId, CancellationToken ct)
// 		{
// 			T aggregate = default(T);
// 			var sql = $"SELECT * FROM {_tableName} WHERE id = @id";
// 			using (var cmd = new NpgsqlCommand(sql, _conn))
// 			{
// 				cmd.Parameters.AddWithValue("id", entityId.ToString());
// 				await using (var reader = await cmd.ExecuteReaderAsync(ct))
// 				{
// 					while (await reader.ReadAsync(ct))
// 					{
// 						aggregate = JsonSerializer.Deserialize<T>(reader.GetFieldValue<string>(1));
// 						aggregate = _migrator.Migrate(aggregate);
// 						break;
// 					}
// 				}				
// 			}
// 			return aggregate;
// 		}
//
// 		public override Task<T> GetFirstOrDefaultWithAsync(Expression<Func<T, bool>> where, CancellationToken ct)
// 			=> throw new NotImplementedException();
// 		
// 		public override async Task<T> GetFirstOrDefaultWithAsync(IEnumerable<(string, object)> andWhere, CancellationToken ct)
// 		{
// 			var aggregates = await GetWithAsync(andWhere, ct);
// 			aggregates = _migrator.Migrate(aggregates);
// 			return aggregates.FirstOrDefault();
// 		}
// 		
// 		public override Task<IEnumerable<T>> GetWithAsync(Expression<Func<T, bool>> where, CancellationToken ct)
// 			=> throw new NotImplementedException();
//
// 		public override async Task<IEnumerable<T>> GetWithAsync(IEnumerable<(string, object)> andWhere, CancellationToken ct)
// 		{
// 			var aggregates = new List<T>();
//
// 			var whereExpr = string.Join(" AND ", andWhere.Select(t => $"data->>'{FormatPropertyName(t.Item1)}' = '{t.Item2}'"));
// 			
// 			var sql = $"SELECT * FROM {_tableName} WHERE {whereExpr}";
// 			using (var cmd = new NpgsqlCommand(sql, _conn))
// 			await using (var reader = await cmd.ExecuteReaderAsync(ct))
// 			{
// 				while (await reader.ReadAsync(ct))
// 				{
// 					var aggregate = JsonSerializer.Deserialize<T>(reader.GetFieldValue<string>(1));
// 					aggregates.Add(aggregate);
// 				}
// 			}
//
// 			aggregates = _migrator.Migrate(aggregates).ToList();
// 			
// 			return aggregates;
// 		}
//
// 		public override async Task SaveAsync(T aggregate, IConnection conn, CancellationToken ct)
// 		{
// 			// Insert some data
// 			var sql = 
// 				$"INSERT INTO {_tableName} (id, data) VALUES (@id, @data) ON CONFLICT (id) " +
// 				$"DO " +
// 				$"UPDATE " +
// 				$"SET data = @data";
// 			
// 			var cmd = new SqlCommand(sql);
//
// 			cmd.Parameters.AddWithValue("id", aggregate.Id.ToString());
// 			cmd.Parameters.AddWithValue("data", JsonSerializer.SerializeToDocument(aggregate));
//
// 			var apa = cmd.ToString();
//
// 			await conn.ExecuteNonQueryAsync(cmd.ToString());
// 			
// 			// var parameters = new QueryParameters();
// 			//
// 			// parameters.Add("id", aggregate.Id.ToString());
// 			// parameters.Add("data", JsonSerializer.SerializeToDocument(aggregate));
//
// 			
// 			
// 			// using (var cmd = new NpgsqlCommand(sql, _conn))
// 			// {
// 			// 	cmd.Parameters.AddWithValue("id", aggregate.Id.ToString());
// 			// 	cmd.Parameters.AddWithValue("data", JsonSerializer.SerializeToDocument(aggregate));
// 			// 	await cmd.ExecuteNonQueryAsync(ct);
// 			// }
// 		}
//
// 		public override Task<string> GetNextIdentityAsync()
// 			=> Task.FromResult(Guid.NewGuid().ToString());
// 	}
// }
