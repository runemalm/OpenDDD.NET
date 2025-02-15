using System.Linq.Expressions;
using OpenDDD.API.Extensions;
using OpenDDD.Domain.Model.Base;
using OpenDDD.Infrastructure.Persistence.OpenDdd.DatabaseSession.InMemory;
using OpenDDD.Infrastructure.Persistence.Serializers;
using OpenDDD.Infrastructure.Repository.OpenDdd.Base;

namespace OpenDDD.Infrastructure.Repository.OpenDdd.InMemory
{
    public class InMemoryOpenDddRepository<TAggregateRoot, TId> 
        : OpenDddRepositoryBase<TAggregateRoot, TId, InMemoryDatabaseSession>
        where TAggregateRoot : AggregateRootBase<TId>
        where TId : notnull
    {
        private readonly InMemoryDatabaseSession _session;
        private readonly string _tableName;

        public InMemoryOpenDddRepository(InMemoryDatabaseSession session, IAggregateSerializer serializer)
            : base(session, serializer)
        {
            _session = session ?? throw new ArgumentNullException(nameof(session));
            _tableName = typeof(TAggregateRoot).Name.ToLower().Pluralize();
        }

        public override async Task<TAggregateRoot?> FindAsync(TId id, CancellationToken ct)
        {
            var serializedData = await _session.LoadAsync<string>(_tableName, id, ct);
            return serializedData != null ? Serializer.Deserialize<TAggregateRoot, TId>(serializedData) : null;
        }

        public override async Task<TAggregateRoot> GetAsync(TId id, CancellationToken ct)
        {
            var entity = await FindAsync(id, ct);
            if (entity == null)
            {
                throw new KeyNotFoundException($"Entity with ID '{id}' was not found in {_tableName}.");
            }
            return entity;
        }

        public override async Task SaveAsync(TAggregateRoot aggregateRoot, CancellationToken ct)
        {
            var serializedData = Serializer.Serialize<TAggregateRoot, TId>(aggregateRoot);
            await _session.SaveAsync(_tableName, aggregateRoot.Id!, serializedData, ct);
        }

        public override async Task DeleteAsync(TAggregateRoot aggregateRoot, CancellationToken ct)
        {
            await _session.DeleteAsync(_tableName, aggregateRoot.Id!, ct);
        }

        public override async Task<IEnumerable<TAggregateRoot>> FindAllAsync(CancellationToken ct)
        {
            var serializedDataList = await _session.LoadAllAsync<string>(_tableName, ct);
            var results = new List<TAggregateRoot>();
            foreach (var serializedData in serializedDataList)
            {
                results.Add(Serializer.Deserialize<TAggregateRoot, TId>(serializedData));
            }
            return results;
        }

        public override async Task<IEnumerable<TAggregateRoot>> FindWithAsync(
            Expression<Func<TAggregateRoot, bool>> filterExpression, CancellationToken ct)
        {
            if (filterExpression == null)
            {
                throw new ArgumentNullException(nameof(filterExpression));
            }

            var allEntities = await FindAllAsync(ct);
            return allEntities.AsQueryable().Where(filterExpression).ToList();
        }
    }
}
