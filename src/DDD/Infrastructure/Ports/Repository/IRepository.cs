using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using DDD.Application;
using DDD.Domain.Model.BuildingBlocks.Aggregate;
using DDD.Domain.Model.BuildingBlocks.Entity;

namespace DDD.Infrastructure.Ports.Repository
{
	public interface IRepository<T> where T : IAggregate
	{
		Task DeleteAllAsync(ActionId actionId, CancellationToken ct);
		Task DeleteAsync(EntityId entityId, ActionId actionId, CancellationToken ct);
		Task<IEnumerable<T>> GetAllAsync(ActionId actionId, CancellationToken ct);
		Task<T> GetAsync(EntityId entityId, ActionId actionId, CancellationToken ct);
		Task<T> GetFirstOrDefaultWithAsync(Expression<Func<T, bool>> where, ActionId actionId, CancellationToken ct);
		Task<T> GetFirstOrDefaultWithAsync(IEnumerable<(string, object)> andWhere, ActionId actionId, CancellationToken ct);
		Task<IEnumerable<T>> GetWithAsync(Expression<Func<T, bool>> where, ActionId actionId, CancellationToken ct);
		Task<IEnumerable<T>> GetWithAsync(IEnumerable<(string, object)> andWhere, ActionId actionId, CancellationToken ct);
		Task SaveAsync(T aggregate, ActionId actionId, CancellationToken ct);
		Task<string> GetNextIdentityAsync();
	}
}
