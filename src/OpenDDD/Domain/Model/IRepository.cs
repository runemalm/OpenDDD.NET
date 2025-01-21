using System.Linq.Expressions;

namespace OpenDDD.Domain.Model
{
    public interface IRepository<TAggregateRoot, TId> where TAggregateRoot : IAggregateRoot<TId>
    {
        Task<TAggregateRoot> GetAsync(TId id, CancellationToken ct);
        Task<TAggregateRoot?> FindAsync(TId id, CancellationToken ct);
        Task<IEnumerable<TAggregateRoot>> FindWithAsync(Expression<Func<TAggregateRoot, bool>> filterExpression, CancellationToken ct);
        Task<IEnumerable<TAggregateRoot>> FindAllAsync(CancellationToken ct);
        Task SaveAsync(TAggregateRoot aggregateRoot, CancellationToken ct);
        Task DeleteAsync(TAggregateRoot aggregateRoot, CancellationToken ct);
    }
}
