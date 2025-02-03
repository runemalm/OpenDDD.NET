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
            return await DbContext.Set<TAggregateRoot>().FindAsync(new object[] { id }, ct);
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
            return await DbContext.Set<TAggregateRoot>().Where(filterExpression).ToListAsync(ct);
        }

        public async Task<IEnumerable<TAggregateRoot>> FindAllAsync(CancellationToken ct)
        {
            return await DbContext.Set<TAggregateRoot>().ToListAsync(ct);
        }
        
        public async Task SaveAsync(TAggregateRoot aggregateRoot, CancellationToken ct)
        {
            if (aggregateRoot == null) throw new ArgumentNullException(nameof(aggregateRoot));

            var dbSet = DbContext.Set<TAggregateRoot>();

            var existingEntity = await dbSet.FindAsync(new object[] { aggregateRoot.Id }, ct);

            if (existingEntity == null)
            {
                await dbSet.AddAsync(aggregateRoot, ct);
            }
            else
            {
                DbContext.Entry(existingEntity).CurrentValues.SetValues(aggregateRoot);
            }
            
            await DbContext.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(TAggregateRoot aggregateRoot, CancellationToken ct)
        {
            DbContext.Set<TAggregateRoot>().Remove(aggregateRoot);
            await DbContext.SaveChangesAsync(ct);
        }
    }
}
