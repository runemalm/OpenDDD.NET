using System;
using System.Collections.Concurrent;
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
		protected BlockingCollection<T> Items = new BlockingCollection<T>();

		public override Task DeleteAllAsync(ActionId actionId, CancellationToken ct)
			=> Task.FromResult(Items = new BlockingCollection<T>());

		public override Task DeleteAsync(EntityId entityId, ActionId actionId, CancellationToken ct)
		{
			Items = new BlockingCollection<T>(
				new ConcurrentQueue<T>(
					Items.Where(i => i.Id != entityId)));
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
				Items =
					new BlockingCollection<T>(
						new ConcurrentQueue<T>(
							Items.Select(i => i.Id == aggregate.Id ? aggregate : i)));
			return Task.CompletedTask;
		}

		public override Task<string> GetNextIdentityAsync()
			=> Task.FromResult(Guid.NewGuid().ToString());
	}
}
