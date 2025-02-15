using Microsoft.EntityFrameworkCore.Storage;
using OpenDDD.Infrastructure.Persistence.DatabaseSession;
using OpenDDD.Infrastructure.Persistence.EfCore.Base;

namespace OpenDDD.Infrastructure.Persistence.EfCore.DatabaseSession
{
    public class EfCoreDatabaseSession : IDatabaseSession
    {
        public readonly OpenDddDbContextBase DbContext;
        private IDbContextTransaction? _transaction;

        public EfCoreDatabaseSession(OpenDddDbContextBase dbContext)
        {
            DbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        }

        public Task OpenConnectionAsync(CancellationToken ct = default)
        {
            // No need to manually open EF Core's connection, so this is a no-op.
            return Task.CompletedTask;
        }

        public async Task BeginTransactionAsync(CancellationToken ct = default)
        {
            if (_transaction == null)
                _transaction = await DbContext.Database.BeginTransactionAsync(ct);
        }

        public async Task CommitTransactionAsync(CancellationToken ct = default)
        {
            if (_transaction == null)
                throw new InvalidOperationException("No transaction in progress.");

            await DbContext.SaveChangesAsync(ct);
            await _transaction.CommitAsync(ct);
            await _transaction.DisposeAsync();
            _transaction = null;
        }

        public async Task RollbackTransactionAsync(CancellationToken ct = default)
        {
            if (_transaction != null)
            {
                await _transaction.RollbackAsync(ct);
                await _transaction.DisposeAsync();
                _transaction = null;
            }
        }
    }
}
