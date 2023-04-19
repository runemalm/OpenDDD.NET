using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using DDD.Application;
using DDD.Application.Error;
using DDD.Application.Settings;
using DDD.Domain.Model.BuildingBlocks.Aggregate;
using DDD.Domain.Model.BuildingBlocks.Entity;
using DDD.Infrastructure.Ports.Adapters.Common.Exceptions;
using DDD.Infrastructure.Ports.Adapters.Common.Translation.Converters;
using DDD.Infrastructure.Ports.Repository;
using DDD.Infrastructure.Services.Persistence;

namespace DDD.Infrastructure.Ports.Adapters.Repository.Sql
{
	public abstract class SqlRepository<T, TId> : Repository<T>, IRepository<T>, IStartableRepository where T : IAggregate where TId : EntityId
	{
		private readonly ISettings _settings;
		private readonly string _tableName;
		private readonly IMigrator<T> _migrator;
		private readonly IPersistenceService _persistenceService;
		private readonly SerializerSettings _serializerSettings;

		public SqlRepository(ISettings settings, string tableName, IMigrator<T> migrator, IPersistenceService persistenceService, SerializerSettings  serializerSettings)
		{
			_settings = settings;
			_tableName = tableName;
			_migrator = migrator;
			_persistenceService = persistenceService;
			_serializerSettings = serializerSettings;
		}
		
		public override void Start(CancellationToken ct)
		{
			AssertTables();
		}

		public override async Task StartAsync(CancellationToken ct)
		{
			await AssertTablesAsync();
		}

		public override void Stop(CancellationToken ct)
		{
			
		}

		public override Task StopAsync(CancellationToken ct)
		{
			return Task.CompletedTask;
		}

		private void AssertTables()
		{
			var stmt = BuildAssertTablesQuery();
			var conn = _persistenceService.GetConnection(ActionId.BootId());
			conn.ExecuteNonQuery(stmt);
		}

		private async Task AssertTablesAsync()
		{
			var stmt = BuildAssertTablesQuery();
			var conn = await _persistenceService.GetConnectionAsync(ActionId.BootId());
			await conn.ExecuteNonQueryAsync(stmt);
		}
		
		private string BuildAssertTablesQuery()
		{
			var stmt =
				$"CREATE TABLE IF NOT EXISTS {_tableName} " +
				$"(id VARCHAR UNIQUE NOT NULL, data json NOT NULL)";
			return stmt;
		}

		public override async Task DeleteAllAsync(ActionId actionId, CancellationToken ct)
		{
			var conn = await _persistenceService.GetConnectionAsync(actionId);
			var stmt = $"DELETE FROM {_tableName}";
			await conn.ExecuteNonQueryAsync(stmt);
			await _persistenceService.ReleaseConnectionAsync(actionId);
		}
		
		public override async Task DeleteAsync(EntityId entityId, ActionId actionId, CancellationToken ct)
		{
			var conn = await _persistenceService.GetConnectionAsync(actionId);
			var stmt = $"DELETE FROM {_tableName} WHERE id = @id";
			var parameters = new Dictionary<string, object>();
			parameters.Add("@id", entityId.ToString());
			var rows = await conn.ExecuteNonQueryAsync(stmt, parameters);
			if (rows != 1)
				throw new PostgresException($"Couldn't delete aggregate, none found with ID '{entityId}'.");
		}
		
		public override IEnumerable<T> GetAll(ActionId actionId, CancellationToken ct)
		{
			var conn = _persistenceService.GetConnection(actionId);
			var stmt = $"SELECT * FROM {_tableName}";
			var aggregates = conn.ExecuteQuery<T>(stmt);
			aggregates = _migrator.Migrate(aggregates).ToList();
			return aggregates;
		}

		public override async Task<IEnumerable<T>> GetAllAsync(ActionId actionId, CancellationToken ct)
		{
			var conn = await _persistenceService.GetConnectionAsync(actionId);
			var stmt = $"SELECT * FROM {_tableName}";
			var aggregates = await conn.ExecuteQueryAsync<T>(stmt);
			aggregates = _migrator.Migrate(aggregates).ToList();
			return aggregates;
		}
		
		public override async Task<T> GetAsync(EntityId entityId, ActionId actionId, CancellationToken ct)
		{
			var conn = await _persistenceService.GetConnectionAsync(actionId);
			
			var stmt = $"SELECT * FROM {_tableName} WHERE id = @id";
			
			var parameters = new Dictionary<string, object>();
			parameters.Add("@id", entityId.ToString());

			var aggregates = await conn.ExecuteQueryAsync<T>(stmt, parameters);
			aggregates = _migrator.Migrate(aggregates).ToList();
			if (aggregates.Count() == 1)
				return aggregates.First();
			if (aggregates.Count() > 1)
				throw new DddException($"Got {aggregates.Count()} aggregates from database for entity ID {entityId}. Expected exactly one.");

			return default(T);
		}
		
		public override async Task<IEnumerable<T>> GetAsync(IEnumerable<EntityId> entityIds, ActionId actionId, CancellationToken ct)
		{
			var conn = await _persistenceService.GetConnectionAsync(actionId);
			
			var stmt = $"SELECT * FROM {_tableName} WHERE id = ANY (@ids)";
			
			var parameters = new Dictionary<string, object>();
			parameters.Add("@ids", entityIds.Select(e => e.ToString()).ToArray());

			var aggregates = await conn.ExecuteQueryAsync<T>(stmt, parameters);
			aggregates = _migrator.Migrate(aggregates).ToList();

			return aggregates;
		}

		public override T GetFirstOrDefaultWith(Expression<Func<T, bool>> where, ActionId actionId, CancellationToken ct)
			=> GetAll(actionId, ct).ToList().FirstOrDefault(where.Compile());

		public override async Task<T> GetFirstOrDefaultWithAsync(Expression<Func<T, bool>> where, ActionId actionId, CancellationToken ct)
			=> (await GetAllAsync(actionId, ct)).ToList().FirstOrDefault(where.Compile());
		
		public override T GetFirstOrDefaultWith(IEnumerable<(string, object)> andWhere, ActionId actionId, CancellationToken ct)
		{
			var aggregates = GetWith(andWhere, actionId, ct);
			aggregates = _migrator.Migrate(aggregates);
			return aggregates.FirstOrDefault();
		}

		public override async Task<T> GetFirstOrDefaultWithAsync(IEnumerable<(string, object)> andWhere, ActionId actionId, CancellationToken ct)
		{
			var aggregates = await GetWithAsync(andWhere, actionId, ct);
			aggregates = _migrator.Migrate(aggregates);
			return aggregates.FirstOrDefault();
		}

		public override IEnumerable<T> GetWith(Expression<Func<T, bool>> where, ActionId actionId, CancellationToken ct)
			=> GetAll(actionId, ct).ToList().Where(where.Compile());

		public override async Task<IEnumerable<T>> GetWithAsync(Expression<Func<T, bool>> where, ActionId actionId, CancellationToken ct)
			=> (await GetAllAsync(actionId, ct)).ToList().Where(where.Compile());
		
		public override IEnumerable<T> GetWith(IEnumerable<(string, object)> andWhere, ActionId actionId, CancellationToken ct)
		{
			var conn = _persistenceService.GetConnection(actionId);
			var whereExpr = string.Join(" AND ", andWhere.Select(t => $"data->>'{FormatPropertyName(t.Item1, _serializerSettings)}' = '{t.Item2}'"));
			var stmt = $"SELECT * FROM {_tableName} WHERE {whereExpr}";
			var aggregates = conn.ExecuteQuery<T>(stmt);
			aggregates = _migrator.Migrate(aggregates).ToList();
			return aggregates;
		}

		public override async Task<IEnumerable<T>> GetWithAsync(IEnumerable<(string, object)> andWhere, ActionId actionId, CancellationToken ct)
		{
			var conn = await _persistenceService.GetConnectionAsync(actionId);
			var whereExpr = string.Join(" AND ", andWhere.Select(t => $"data->>'{FormatPropertyName(t.Item1, _serializerSettings)}' = '{t.Item2}'"));
			var stmt = $"SELECT * FROM {_tableName} WHERE {whereExpr}";
			var aggregates = await conn.ExecuteQueryAsync<T>(stmt);
			aggregates = _migrator.Migrate(aggregates).ToList();
			return aggregates;
		}
		
		public override void Save(T aggregate, ActionId actionId, CancellationToken ct)
		{
			var conn = _persistenceService.GetConnection(actionId);
			var (query, parameters) = BuildSaveQueryAndParameters(aggregate, actionId, ct);
			conn.ExecuteNonQuery(query, parameters);
		}

		public override async Task SaveAsync(T aggregate, ActionId actionId, CancellationToken ct)
		{
			var conn = await _persistenceService.GetConnectionAsync(actionId);
			var (query, parameters) = BuildSaveQueryAndParameters(aggregate, actionId, ct);
			await conn.ExecuteNonQueryAsync(query, parameters);
		}

		private (string, IDictionary<string, object>) BuildSaveQueryAndParameters(T aggregate, ActionId actionId, CancellationToken ct)
		{
			var query =
				$"INSERT INTO {_tableName} (id, data) VALUES (@id, @data) ON CONFLICT (id) " +
				$"DO " +
				$"UPDATE " +
				$"SET data = @data";
			
			var parameters = new Dictionary<string, object>();
			parameters.Add("@id", aggregate.Id.ToString());
			parameters.Add("@data", JsonDocument.Parse(JsonConvert.SerializeObject(aggregate, _serializerSettings)));

			return (query, parameters);
		}

		public override string GetNextIdentity()
			=> Guid.NewGuid().ToString();

		public override Task<string> GetNextIdentityAsync()
			=> Task.FromResult(GetNextIdentity());
	}
}
