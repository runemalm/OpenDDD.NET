using OpenDDD.Domain.Model;
using OpenDDD.Domain.Model.Base;

namespace OpenDDD.Infrastructure.Persistence.UoW
{
    public interface IUnitOfWork : IDisposable
    {
        Task StartAsync(CancellationToken ct);
        Task CommitAsync(CancellationToken ct);
        Task RollbackAsync(CancellationToken ct);

        Task AddToOutboxAsync(IEvent @event, CancellationToken ct);
        Task SaveAsync<TAggregateRoot, TId>(TAggregateRoot aggregateRoot, CancellationToken ct)
            where TAggregateRoot : AggregateRootBase<TId>;
    }
}
