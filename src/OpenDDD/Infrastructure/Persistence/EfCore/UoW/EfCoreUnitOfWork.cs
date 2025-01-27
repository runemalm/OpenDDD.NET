using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using OpenDDD.Domain.Model;
using OpenDDD.Infrastructure.Persistence.UoW;

namespace OpenDDD.Infrastructure.Persistence.EfCore.UoW
{
    public class EfCoreUnitOfWork : UnitOfWorkBase
    {
        public readonly DbContext DbContext;
        private IDbContextTransaction? _currentTransaction;
        private bool _isInMemoryDatabase;

        public EfCoreUnitOfWork(DbContext dbContext)
        {
            DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
            _isInMemoryDatabase = dbContext.Database.IsInMemory();
        }

        protected override async Task BeginTransactionInternalAsync(CancellationToken ct)
        {
            if (_isInMemoryDatabase)
            {
                _currentTransaction = null;
                return;
            }

            if (_currentTransaction != null)
            {
                throw new InvalidOperationException("A transaction is already in progress.");
            }

            _currentTransaction = await DbContext.Database.BeginTransactionAsync(ct);
        }

        protected override async Task CommitTransactionInternalAsync(CancellationToken ct)
        {
            if (_isInMemoryDatabase)
            {
                await DbContext.SaveChangesAsync(ct);
                return;
            }

            if (_currentTransaction == null)
            {
                throw new InvalidOperationException("No transaction in progress.");
            }

            try
            {
                await DbContext.SaveChangesAsync(ct);
                await _currentTransaction.CommitAsync(ct);
            }
            finally
            {
                DisposeTransaction();
            }
        }

        protected override async Task RollbackTransactionInternalAsync(CancellationToken ct)
        {
            if (_isInMemoryDatabase)
            {
                return;
            }

            if (_currentTransaction != null)
            {
                await _currentTransaction.RollbackAsync(ct);
                DisposeTransaction();
            }
        }

        public override Task AddToOutboxAsync(IEvent @event, CancellationToken ct)
        {
            throw new NotImplementedException();
        }

        public override async Task SaveAsync<TAggregateRoot, TId>(TAggregateRoot aggregateRoot, CancellationToken ct)
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
        }

        protected override void DisposeInternal()
        {
            DisposeTransaction();
            DbContext.Dispose();
        }

        private void DisposeTransaction()
        {
            if (!_isInMemoryDatabase)
            {
                _currentTransaction?.Dispose();
            }

            _currentTransaction = null;
        }
    }
}
