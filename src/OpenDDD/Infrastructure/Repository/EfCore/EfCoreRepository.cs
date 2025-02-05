using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using OpenDDD.Domain.Model;
using OpenDDD.Domain.Model.Base;
using OpenDDD.Infrastructure.Persistence.EfCore.UoW;
using OpenDDD.Infrastructure.Persistence.UoW;

namespace OpenDDD.Infrastructure.Repository.EfCore
{
    public class EfCoreRepository<TAggregateRoot, TId> : IRepository<TAggregateRoot, TId>
        where TAggregateRoot : AggregateRootBase<TId>
        where TId : notnull
    {
        private readonly IUnitOfWork _unitOfWork;
        protected readonly DbContext DbContext;

        public EfCoreRepository(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));

            if (_unitOfWork is not EfCoreUnitOfWork efCoreUnitOfWork)
            {
                throw new InvalidOperationException($"The provided IUnitOfWork is not of type {nameof(EfCoreUnitOfWork)}.");
            }

            DbContext = efCoreUnitOfWork.DbContext;
        }

        public async Task<TAggregateRoot?> FindAsync(TId id, CancellationToken ct)
        {
            return await DbContext.Set<TAggregateRoot>()
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id.Equals(id), ct);
        }

        public async Task<TAggregateRoot> GetAsync(TId id, CancellationToken ct)
        {
            var entity = await FindAsync(id, ct);
            if (entity == null)
            {
                throw new KeyNotFoundException($"Entity with ID '{id}' was not found.");
            }
            return entity;
        }

        public async Task<IEnumerable<TAggregateRoot>> FindWithAsync(Expression<Func<TAggregateRoot, bool>> filterExpression, CancellationToken ct)
        {
            return await DbContext.Set<TAggregateRoot>()
                .AsNoTracking()
                .Where(filterExpression)
                .ToListAsync(ct);
        }

        public async Task<IEnumerable<TAggregateRoot>> FindAllAsync(CancellationToken ct)
        {
            return await DbContext.Set<TAggregateRoot>()
                .AsNoTracking()
                .ToListAsync(ct);
        }

        public async Task SaveAsync(TAggregateRoot aggregateRoot, CancellationToken ct)
        {
            if (aggregateRoot == null) throw new ArgumentNullException(nameof(aggregateRoot));

            var dbSet = DbContext.Set<TAggregateRoot>();

            // Check if entity exists
            var exists = await dbSet.AsNoTracking().AnyAsync(e => e.Id.Equals(aggregateRoot.Id), ct);

            if (!exists)
            {
                await dbSet.AddAsync(aggregateRoot, ct);
            }
            else
            {
                // Remove any existing tracked instance to avoid conflicts
                var trackedEntity = DbContext.ChangeTracker.Entries<TAggregateRoot>()
                    .FirstOrDefault(e => e.Entity.Id.Equals(aggregateRoot.Id));

                if (trackedEntity != null)
                {
                    DbContext.Entry(trackedEntity.Entity).State = EntityState.Detached;
                }

                // Attach and mark as modified
                dbSet.Attach(aggregateRoot);
                DbContext.Entry(aggregateRoot).State = EntityState.Modified;

                // Ensure owned entities are also marked as modified
                foreach (var navigation in DbContext.Entry(aggregateRoot).Metadata.GetNavigations())
                {
                    if (navigation.TargetEntityType.IsOwned())
                    {
                        var ownedEntity = DbContext.Entry(aggregateRoot).Navigation(navigation.Name).CurrentValue;
                        if (ownedEntity != null)
                        {
                            DbContext.Entry(ownedEntity).State = EntityState.Modified;
                        }
                    }
                }
            }

            await DbContext.SaveChangesAsync(ct);

            // Detach the entity to avoid tracking issues
            DbContext.Entry(aggregateRoot).State = EntityState.Detached;
        }

        public async Task DeleteAsync(TAggregateRoot aggregateRoot, CancellationToken ct)
        {
            if (aggregateRoot == null) throw new ArgumentNullException(nameof(aggregateRoot));

            var dbSet = DbContext.Set<TAggregateRoot>();
            dbSet.Attach(aggregateRoot);
            dbSet.Remove(aggregateRoot);

            await DbContext.SaveChangesAsync(ct);
        }
    }
}
