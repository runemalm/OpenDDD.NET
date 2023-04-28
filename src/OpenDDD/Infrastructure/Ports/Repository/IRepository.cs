using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using OpenDDD.Application;
using OpenDDD.Domain.Model.BuildingBlocks.Aggregate;
using OpenDDD.Domain.Model.BuildingBlocks.Entity;

namespace OpenDDD.Infrastructure.Ports.Repository
{
	public interface IRepository<T> where T : IAggregate
	{
		void Start(CancellationToken ct);
		Task StartAsync(CancellationToken ct);
		void Stop(CancellationToken ct);
		Task StopAsync(CancellationToken ct);
		void DeleteAll(ActionId actionId, CancellationToken ct);
		Task DeleteAllAsync(ActionId actionId, CancellationToken ct);
		Task DeleteAsync(EntityId entityId, ActionId actionId, CancellationToken ct);
		IEnumerable<T> GetAll(ActionId actionId, CancellationToken ct);
		Task<IEnumerable<T>> GetAllAsync(ActionId actionId, CancellationToken ct);
		Task<T> GetAsync(EntityId entityId, ActionId actionId, CancellationToken ct);
		Task<IEnumerable<T>> GetAsync(IEnumerable<EntityId> entityIds, ActionId actionId, CancellationToken ct);
		T GetFirstOrDefaultWith(Expression<Func<T, bool>> where, ActionId actionId, CancellationToken ct);
		Task<T> GetFirstOrDefaultWithAsync(Expression<Func<T, bool>> where, ActionId actionId, CancellationToken ct);
		T GetFirstOrDefaultWith(IEnumerable<(string, object)> andWhere, ActionId actionId, CancellationToken ct);
		Task<T> GetFirstOrDefaultWithAsync(IEnumerable<(string, object)> andWhere, ActionId actionId, CancellationToken ct);
		IEnumerable<T> GetWith(Expression<Func<T, bool>> where, ActionId actionId, CancellationToken ct);
		Task<IEnumerable<T>> GetWithAsync(Expression<Func<T, bool>> where, ActionId actionId, CancellationToken ct);
		IEnumerable<T> GetWith(IEnumerable<(string, object)> andWhere, ActionId actionId, CancellationToken ct);
		Task<IEnumerable<T>> GetWithAsync(IEnumerable<(string, object)> andWhere, ActionId actionId, CancellationToken ct);
		void Save(T aggregate, ActionId actionId, CancellationToken ct);
		Task SaveAsync(T aggregate, ActionId actionId, CancellationToken ct);
		string GetNextIdentity();
		Task<string> GetNextIdentityAsync();
	}
}
