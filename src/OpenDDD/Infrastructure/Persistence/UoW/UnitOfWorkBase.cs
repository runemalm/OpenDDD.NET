using OpenDDD.Domain.Model;
using OpenDDD.Domain.Model.Base;

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

        public abstract Task AddToOutboxAsync(IEvent @event, CancellationToken ct);
        public abstract Task SaveAsync<TAggregateRoot, TId>(TAggregateRoot aggregateRoot, CancellationToken ct)
            where TAggregateRoot : AggregateRootBase<TId>;

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
