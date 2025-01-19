using System.Linq.Expressions;

namespace OpenDDD.Domain.Model
{
    public interface IRepository<TAggregateRoot, TId> where TAggregateRoot : IAggregateRoot<TId>
    {
        Task<TAggregateRoot> GetAsync(TId id);
        Task<TAggregateRoot?> FindAsync(TId id);
        Task<IEnumerable<TAggregateRoot>> FindWithAsync(Expression<Func<TAggregateRoot, bool>> filterExpression);
        Task<IEnumerable<TAggregateRoot>> FindAllAsync();
        Task SaveAsync(TAggregateRoot aggregateRoot, CancellationToken ct);
        Task DeleteAsync(TAggregateRoot aggregateRoot);
    }
}
