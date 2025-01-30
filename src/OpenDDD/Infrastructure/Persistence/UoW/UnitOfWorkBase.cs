namespace OpenDDD.Infrastructure.Persistence.UoW
{
    public abstract class UnitOfWorkBase : IUnitOfWork
    {
        private bool _disposed = false;

        public async Task StartAsync(CancellationToken ct)
        {
            await BeginTransactionInternalAsync(ct);
        }

        public async Task CommitAsync(CancellationToken ct)
        {
            try
            {
                await CommitTransactionInternalAsync(ct);
            }
            catch
            {
                await RollbackAsync(ct);
                throw;
            }
        }

        public async Task RollbackAsync(CancellationToken ct)
        {
            await RollbackTransactionInternalAsync(ct);
        }

        protected abstract Task BeginTransactionInternalAsync(CancellationToken ct);
        protected abstract Task CommitTransactionInternalAsync(CancellationToken ct);
        protected abstract Task RollbackTransactionInternalAsync(CancellationToken ct);

        public void Dispose()
        {
            if (!_disposed)
            {
                DisposeInternal();
                _disposed = true;
            }
        }

        protected abstract void DisposeInternal();
    }
}
