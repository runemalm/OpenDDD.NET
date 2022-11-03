using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using DDD.Application;
using DDD.Application.Exceptions;
using DDD.Application.Settings;
using DDD.Domain.Model.BuildingBlocks.Aggregate;
using DDD.Domain.Model.BuildingBlocks.Entity;
using DDD.Infrastructure.Ports.Adapters.Common.Exceptions;
using DDD.Infrastructure.Ports.Repository;
using DDD.Infrastructure.Services.Persistence;
using JsonSerializer = System.Text.Json.JsonSerializer;

namespace DDD.Infrastructure.Ports.Adapters.Repository.Sql
{
	public abstract class SqlRepository<T, TId> : Repository<T>, IRepository<T> where T : IAggregate where TId : EntityId
	{
		private readonly ISettings _settings;
		private readonly string _tableName;
		private readonly IMigrator<T> _migrator;
		private readonly IPersistenceService _persistenceService;

		public SqlRepository(ISettings settings, string tableName, IMigrator<T> migrator, IPersistenceService persistenceService)
		{
			_settings = settings;
			_tableName = tableName;
			_migrator = migrator;
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
				$"CREATE TABLE IF NOT EXISTS {_tableName} " +
				$"(id VARCHAR UNIQUE NOT NULL,data json NOT NULL)";

			using (var conn = await _persistenceService.OpenConnectionAsync())
				await conn.ExecuteNonQueryAsync(stmt);
		}

		public override async Task DeleteAllAsync(ActionId actionId, CancellationToken ct)
		{
			var conn = await _persistenceService.GetConnectionAsync(actionId);
			var stmt = $"DELETE FROM {_tableName}";
			await conn.ExecuteNonQueryAsync(stmt);
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

		public override Task<T> GetFirstOrDefaultWithAsync(Expression<Func<T, bool>> where, ActionId actionId, CancellationToken ct)
			=> throw new NotImplementedException();
		
		public override async Task<T> GetFirstOrDefaultWithAsync(IEnumerable<(string, object)> andWhere, ActionId actionId, CancellationToken ct)
		{
			var aggregates = await GetWithAsync(andWhere, actionId, ct);
			aggregates = _migrator.Migrate(aggregates);
			return aggregates.FirstOrDefault();
		}
		
		public override Task<IEnumerable<T>> GetWithAsync(Expression<Func<T, bool>> where, CancellationToken ct)
			=> throw new NotImplementedException();
		
		public override async Task<IEnumerable<T>> GetWithAsync(IEnumerable<(string, object)> andWhere, ActionId actionId, CancellationToken ct)
		{
			var conn = await _persistenceService.GetConnectionAsync(actionId);
			var whereExpr = string.Join(" AND ", andWhere.Select(t => $"data->>'{FormatPropertyName(t.Item1)}' = '{t.Item2}'"));
			var stmt = $"SELECT * FROM {_tableName} WHERE {whereExpr}";
			var aggregates = await conn.ExecuteQueryAsync<T>(stmt);
			aggregates = _migrator.Migrate(aggregates).ToList();
			return aggregates;
		}
		
		public override async Task SaveAsync(T aggregate, ActionId actionId, CancellationToken ct)
		{
			var conn = await _persistenceService.GetConnectionAsync(actionId);
				
			var stmt =
				$"INSERT INTO {_tableName} (id, data) VALUES (@id, @data) ON CONFLICT (id) " +
				$"DO " +
				$"UPDATE " +
				$"SET data = @data";
			
			var parameters = new Dictionary<string, object>();
			parameters.Add("@id", aggregate.Id.ToString());
			parameters.Add("@data", JsonSerializer.SerializeToDocument(aggregate));
			await conn.ExecuteNonQueryAsync(stmt, parameters);
		}

		public override Task<string> GetNextIdentityAsync()
			=> Task.FromResult(Guid.NewGuid().ToString());
	}
}
