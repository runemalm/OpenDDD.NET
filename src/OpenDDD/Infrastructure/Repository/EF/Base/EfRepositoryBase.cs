using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using OpenDDD.Domain.Model;
using OpenDDD.Domain.Model.Base;

namespace OpenDDD.Infrastructure.Repository.EF.Base
{
    public abstract class EfRepositoryBase<TAggregateRoot, TId, TContext> : IRepository<TAggregateRoot, TId>
        where TAggregateRoot : AggregateRootBase<TId>
        where TContext : EfDbContextBase
    {
        protected readonly TContext _context;
        protected readonly DbSet<TAggregateRoot> DbSet;

        protected EfRepositoryBase(TContext context)
        {
            _context = context;
            DbSet = context.Set<TAggregateRoot>();
        }
        
        protected virtual IQueryable<TAggregateRoot> IncludeRelatedEntities(IQueryable<TAggregateRoot> query)
        {
            return query;
        }
        
        protected virtual void AttachRelatedEntities(TAggregateRoot aggregateRoot)
        {
            
        }

        public async Task<TAggregateRoot> GetAsync(TId id)
        {
            _context.ChangeTracker.Clear(); // Clear any cached tracked entities
            var query = IncludeRelatedEntities(DbSet.AsNoTracking());
            return await query.FirstOrDefaultAsync(e => e.Id != null && e.Id.Equals(id)) 
                   ?? throw new InvalidOperationException($"Aggregate root with ID {id} not found.");
        }

        public async Task<TAggregateRoot?> FindAsync(TId id)
        {
            var query = IncludeRelatedEntities(DbSet.AsNoTracking());
            return await query.FirstOrDefaultAsync(e => e.Id != null && e.Id.Equals(id));
        }

        public async Task<IEnumerable<TAggregateRoot>> FindWithAsync(Expression<Func<TAggregateRoot, bool>> filterExpression)
        {
            var query = IncludeRelatedEntities(DbSet.AsNoTracking());
            return await query.Where(filterExpression).ToListAsync();
        }

        public async Task<IEnumerable<TAggregateRoot>> FindAllAsync()
        {
            var query = IncludeRelatedEntities(DbSet.AsNoTracking());
            return await query.ToListAsync();
        }

        public async Task SaveAsync(TAggregateRoot aggregateRoot, CancellationToken ct)
        {
            if (EqualityComparer<TId>.Default.Equals(aggregateRoot.Id, default))
            {
                throw new InvalidOperationException("Cannot save an aggregate root with an unset ID.");
            }

            // Force entity to be tracked as modified if detached
            var entry = _context.Entry(aggregateRoot);
            if (entry.State == EntityState.Detached)
            {
                var exists = await DbSet.AsNoTracking().AnyAsync(e => e.Id != null && e.Id.Equals(aggregateRoot.Id), ct);

                if (exists)
                {
                    DbSet.Attach(aggregateRoot);
                    entry.State = EntityState.Modified; // Ensure changes are persisted
                    
                    AttachRelatedEntities(aggregateRoot);
                }
                else
                {
                    await DbSet.AddAsync(aggregateRoot, ct); // New entity
                }
            }

            await _context.SaveChangesAsync(ct);
        }

        public async Task DeleteAsync(TAggregateRoot aggregateRoot)
        {
            var exists = await DbSet.AsNoTracking().AnyAsync(e => e.Id != null && e.Id.Equals(aggregateRoot.Id));
            if (!exists)
                throw new InvalidOperationException("Cannot delete an aggregate root that does not exist.");

            DbSet.Remove(aggregateRoot);
            await _context.SaveChangesAsync();
        }
    }
}
