using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Namotion.Reflection;
using OpenDDD.Application;
using OpenDDD.Domain.Model.BuildingBlocks.Aggregate;
using OpenDDD.Domain.Model.BuildingBlocks.Entity;
using OpenDDD.Infrastructure.Ports.Repository;

namespace OpenDDD.Infrastructure.Ports.Adapters.Repository.Memory
{
	public abstract class MemoryRepository<T> : Repository<T>, IRepository<T>, IStartableRepository where T : IAggregate
	{
		protected BlockingCollection<T> Items = new BlockingCollection<T>();
		
		public override void Start(CancellationToken ct)
		{
			
		}

		public override Task StartAsync(CancellationToken ct)
		{
			return Task.CompletedTask;
		}

		public override void Stop(CancellationToken ct)
		{
			
		}

		public override Task StopAsync(CancellationToken ct)
		{
			return Task.CompletedTask;
		}

		public override void DeleteAll(ActionId actionId, CancellationToken ct)
			=> Task.FromResult(Items = new BlockingCollection<T>());

		public override Task DeleteAllAsync(ActionId actionId, CancellationToken ct)
		{
			DeleteAll(actionId, ct);
			return Task.CompletedTask;
		}

		public override Task DeleteAsync(EntityId entityId, ActionId actionId, CancellationToken ct)
		{
			Items = new BlockingCollection<T>(
				new ConcurrentQueue<T>(
					Items.Where(i => i.Id != entityId)));
			return Task.CompletedTask;
		}

		public override IEnumerable<T> GetAll(ActionId actionId, CancellationToken ct)
			=> Items;

		public override async Task<IEnumerable<T>> GetAllAsync(ActionId actionId, CancellationToken ct)
			=> await Task.FromResult(GetAll(actionId, ct));
		
		public override T Get(EntityId entityId, ActionId actionId, CancellationToken ct)
			=> Items.FirstOrDefault(i => i.Id == entityId);

		public override async Task<T> GetAsync(EntityId entityId, ActionId actionId, CancellationToken ct)
			=> await Task.FromResult(Get(entityId, actionId, ct));
		
		public override async Task<IEnumerable<T>> GetAsync(IEnumerable<EntityId> entityIds, ActionId actionId, CancellationToken ct)
			=> await Task.FromResult(Items.Where(i => entityIds.Contains(i.Id)));

		public override T GetFirstOrDefaultWith(Expression<Func<T, bool>> where, ActionId actionId, CancellationToken ct)
			=> Items.Where(where.Compile()).FirstOrDefault();

		public override Task<T> GetFirstOrDefaultWithAsync(Expression<Func<T, bool>> where, ActionId actionId, CancellationToken ct)
			=> Task.FromResult(GetFirstOrDefaultWith(where, actionId, ct));

		public override T GetFirstOrDefaultWith(IEnumerable<(string, object)> andWhere, ActionId actionId, CancellationToken ct)
			=> GetWith(andWhere, actionId, ct).FirstOrDefault();

		public override async Task<T> GetFirstOrDefaultWithAsync(IEnumerable<(string, object)> andWhere, ActionId actionId, CancellationToken ct)
			=> (await GetWithAsync(andWhere, actionId, ct)).FirstOrDefault();

		public override IEnumerable<T> GetWith(Expression<Func<T, bool>> where, ActionId actionId, CancellationToken ct)
			=> Items.Where(where.Compile()).ToList().AsEnumerable();

		public override Task<IEnumerable<T>> GetWithAsync(Expression<Func<T, bool>> where, ActionId actionId, CancellationToken ct)
			=> Task.FromResult(GetWith(where, actionId, ct));
		
		public override IEnumerable<T> GetWith(IEnumerable<(string, object)> andWhere, ActionId actionId, CancellationToken ct)
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

			return filtered.AsEnumerable();
		}

		public override Task<IEnumerable<T>> GetWithAsync(IEnumerable<(string, object)> andWhere, ActionId actionId, CancellationToken ct)
			=> Task.FromResult(GetWith(andWhere, actionId, ct));

		public override void Save(T aggregate, ActionId actionId, CancellationToken ct)
		{
			if (!Items.Contains(aggregate))
				Items.Add(aggregate);
			else
				Items =
					new BlockingCollection<T>(
						new ConcurrentQueue<T>(
							Items.Select(i => i.Id == aggregate.Id ? aggregate : i)));
		}

		public override Task SaveAsync(T aggregate, ActionId actionId, CancellationToken ct)
		{
			Save(aggregate, actionId, ct);
			return Task.CompletedTask;
		}

		public override string GetNextIdentity()
			=> Guid.NewGuid().ToString();

		public override Task<string> GetNextIdentityAsync()
			=> Task.FromResult(GetNextIdentity());
	}
}
