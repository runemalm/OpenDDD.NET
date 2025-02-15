using System.Linq.Expressions;
using OpenDDD.Domain.Model;
using OpenDDD.Domain.Model.Base;
using OpenDDD.Infrastructure.Persistence.Serializers;

namespace OpenDDD.Infrastructure.Repository.OpenDdd.Base
{
    public abstract class OpenDddRepositoryBase<TAggregateRoot, TId, TDatabaseSession> : IRepository<TAggregateRoot, TId>
        where TAggregateRoot : AggregateRootBase<TId>
        where TId : notnull
    {
        protected readonly TDatabaseSession DatabaseSession;
        protected readonly IAggregateSerializer Serializer;

        protected OpenDddRepositoryBase(TDatabaseSession databaseSession, IAggregateSerializer serializer)
        {
            DatabaseSession = databaseSession ?? throw new ArgumentNullException(nameof(databaseSession));
            Serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
        }

        public abstract Task<TAggregateRoot> GetAsync(TId id, CancellationToken ct);
        public abstract Task<TAggregateRoot?> FindAsync(TId id, CancellationToken ct);
        public abstract Task<IEnumerable<TAggregateRoot>> FindWithAsync(Expression<Func<TAggregateRoot, bool>> filterExpression, CancellationToken ct);
        public abstract Task<IEnumerable<TAggregateRoot>> FindAllAsync(CancellationToken ct);
        public abstract Task SaveAsync(TAggregateRoot aggregateRoot, CancellationToken ct);
        public abstract Task DeleteAsync(TAggregateRoot aggregateRoot, CancellationToken ct);
    }
}
