using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using OpenDDD.Application;
using OpenDDD.Domain.Model.AggregateRoot;
using OpenDDD.Domain.Model.Entity;
using OpenDDD.Infrastructure.Ports.Database;

namespace OpenDDD.Infrastructure.Ports.Repository
{
	public interface IRepository<TEntity, TEntityId, TDatabaseConnection> 
		where TEntity : IAggregateRoot 
		where TEntityId : EntityId
		where TDatabaseConnection : IDatabaseConnection
	{
		TDatabaseConnection DatabaseConnection { get; set; }
		
		void DeleteAll(ActionId actionId, CancellationToken ct);
		Task DeleteAllAsync(ActionId actionId, CancellationToken ct);
		// Task DeleteAsync(TId entityId, ActionId actionId, CancellationToken ct);
		IEnumerable<TEntity> GetAll(ActionId actionId, CancellationToken ct);
		Task<IEnumerable<TEntity>> GetAllAsync(ActionId actionId, CancellationToken ct);
		// TEntity Get(TId entityId, ActionId actionId, CancellationToken ct);
		Task<TEntity> GetAsync(TEntityId entityId, ActionId actionId, CancellationToken ct);
		// Task<IEnumerable<TEntity>> GetAsync(IEnumerable<TId> entityIds, ActionId actionId, CancellationToken ct);
		// TEntity GetFirstOrDefaultWith(Expression<Func<TEntity, bool>> where, ActionId actionId, CancellationToken ct);
		Task<TEntity> GetFirstOrDefaultWithAsync(Expression<Func<TEntity, bool>> where, ActionId actionId, CancellationToken ct);
		// TEntity GetFirstOrDefaultWith(IEnumerable<(string, object)> andWhere, ActionId actionId, CancellationToken ct);
		// Task<TEntity> GetFirstOrDefaultWithAsync(IEnumerable<(string, object)> andWhere, ActionId actionId, CancellationToken ct);
		// IEnumerable<TEntity> GetWith(Expression<Func<TEntity, bool>> where, ActionId actionId, CancellationToken ct);
		// Task<IEnumerable<TEntity>> GetWithAsync(Expression<Func<TEntity, bool>> where, ActionId actionId, CancellationToken ct);
		// IEnumerable<TEntity> GetWith(IEnumerable<(string, object)> andWhere, ActionId actionId, CancellationToken ct);
		// Task<IEnumerable<TEntity>> GetWithAsync(IEnumerable<(string, object)> andWhere, ActionId actionId, CancellationToken ct);
		void Save(TEntity aggregate, ActionId actionId, CancellationToken ct);
		Task SaveAsync(TEntity aggregate, ActionId actionId, CancellationToken ct);
		// string GetNextIdentity();
		// Task<string> GetNextIdentityAsync();
	}
}
