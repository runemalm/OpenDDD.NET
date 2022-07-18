using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using DDD.Domain;

namespace DDD.Infrastructure.Ports.Adapters.Memory
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

		public override Task<T> GetFirstOrDefaultWithAsync(Expression<Func<T, bool>> where, ActionId actionId, CancellationToken ct)
			=> Task.FromResult(Items.Where(where.Compile()).FirstOrDefault());
		
		public override Task<T> GetFirstOrDefaultWithAsync(IEnumerable<(string, object)> andWhere, ActionId actionId, CancellationToken ct)
			=> throw new NotImplementedException();

		public override Task<IEnumerable<T>> GetWithAsync(Expression<Func<T, bool>> where, CancellationToken ct)
			=> Task.FromResult(Items.Where(where.Compile()));
		
		public override Task<IEnumerable<T>> GetWithAsync(IEnumerable<(string, object)> andWhere, ActionId actionId, CancellationToken ct)
			=> throw new NotImplementedException();

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
