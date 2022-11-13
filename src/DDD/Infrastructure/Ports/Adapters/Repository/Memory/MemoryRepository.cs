using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using DDD.Application;
using DDD.Domain.Model.BuildingBlocks.Aggregate;
using DDD.Domain.Model.BuildingBlocks.Entity;
using DDD.Infrastructure.Ports.Repository;
using Namotion.Reflection;

namespace DDD.Infrastructure.Ports.Adapters.Repository.Memory
{
	public abstract class MemoryRepository<T> : Repository<T>, IRepository<T> where T : IAggregate
	{
		protected ICollection<T> Items = new List<T>();

		public override Task DeleteAllAsync(ActionId actionId, CancellationToken ct)
			=> Task.FromResult(Items = new List<T>());

		public override Task DeleteAsync(EntityId entityId, ActionId actionId, CancellationToken ct)
		{
			Items = Items.Where(i => i.Id != entityId).ToList();
			return Task.CompletedTask;
		}

		public override async Task<IEnumerable<T>> GetAllAsync(ActionId actionId, CancellationToken ct)
			=> await Task.FromResult(Items);
		
		public override async Task<T> GetAsync(EntityId entityId, ActionId actionId, CancellationToken ct)
			=> await Task.FromResult(Items.FirstOrDefault(i => i.Id == entityId));
		
		public override async Task<IEnumerable<T>> GetAsync(IEnumerable<EntityId> entityIds, ActionId actionId, CancellationToken ct)
			=> await Task.FromResult(Items.Where(i => entityIds.Contains(i.Id)));

		public override Task<T> GetFirstOrDefaultWithAsync(Expression<Func<T, bool>> where, ActionId actionId, CancellationToken ct)
			=> Task.FromResult(Items.Where(where.Compile()).FirstOrDefault());

		public override async Task<T> GetFirstOrDefaultWithAsync(IEnumerable<(string, object)> andWhere, ActionId actionId,
			CancellationToken ct)
			=> (await GetWithAsync(andWhere, actionId, ct)).FirstOrDefault();

		public override Task<IEnumerable<T>> GetWithAsync(Expression<Func<T, bool>> where, ActionId actionId, CancellationToken ct)
			=> Task.FromResult(Items.Where(where.Compile()).ToList().AsEnumerable());
		
		public override Task<IEnumerable<T>> GetWithAsync(IEnumerable<(string, object)> andWhere, ActionId actionId, CancellationToken ct)
		{
			var filtered = new List<T>();
			
			foreach (var item in Items)
			{
				foreach (var tuple in andWhere)
				{
					var value = item.TryGetPropertyValue<string>(tuple.Item1);
					if (value == tuple.Item2)
						filtered.Add(item);
				}
			}

			return Task.FromResult(filtered.AsEnumerable());
		}

		public override Task SaveAsync(T aggregate, ActionId actionId, CancellationToken ct)
		{
			if (!Items.Contains(aggregate))
				Items.Add(aggregate);
			else
				Items = Items.Select(i => i.Id == aggregate.Id ? aggregate : i).ToList();
			return Task.CompletedTask;
		}

		public override Task<string> GetNextIdentityAsync()
			=> Task.FromResult(Guid.NewGuid().ToString());


		
		// TODO: Use below when implementing the linq-based methods.

		//public async Task<T> GetAsync(string id, CancellationToken ct)
		//	=> await Task.FromResult(Items.FirstOrDefault(i => i.Id == id));

		//public async Task CreateAsync(IEnumerable<T> items, CancellationToken ct)
		//{
		//	foreach (var i in items)
		//		Items = Items.Append<T>(i);
		//	await Task.CompletedTask;
		//}

		//public async Task CreateAsync(T item, CancellationToken ct)
		//{
		//	if (Items.FirstOrDefault(i => i.Id == item.Id) != null)
		//		throw new Exception("Set a unique Id");
		//	Items = Items.Append<T>(item);
		//	await Task.CompletedTask;
		//}

		//public async Task UpdateAsync(T item, CancellationToken ct)
		//	=> await Task.FromResult(Items = Items.Select(i => i.Id == item.Id ? item : i));

		//public async Task UpsertAsync(T item, CancellationToken ct)
		//{
		//	var exists = (await GetAsync(item.Id, ct)) != null;
		//	if (!exists)
		//		Items = Items.Append<T>(item);
		//	else
		//		await UpdateAsync(item, ct);
		//}

		//public Task DeleteAsync(Expression<Func<T, bool>> where, CancellationToken ct)
		//	=> Task.FromResult(Items = Items.Except(Items.Where(where.Compile())));

		//public Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> where, CancellationToken ct)
		//	=> Task.FromResult(Items.Where(where.Compile()));

		//public async Task<T> GetFirstAsync(
		//	CancellationToken ct,
		//	string sortOrder,
		//	Expression<Func<T, object>> sortBy,
		//	Expression<Func<T, bool>> where = null)
		//{
		//	var filtered = await FindAsync(where ?? (_ => true), ct);
		//	var sorted =
		//		sortOrder == "Ascending"
		//			? filtered.OrderBy(sortBy.Compile())
		//			: filtered.OrderByDescending(sortBy.Compile());
		//	return sorted.FirstOrDefault();
		//}

		//public async Task<T> FindOneAsync(Expression<Func<T, bool>> where, CancellationToken ct)
		//	=> (await FindAsync(where, ct)).FirstOrDefault();
	}
}
