using System.Collections.Concurrent;
using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OpenDDD.Domain.Model;
using OpenDDD.Domain.Model.Base;

namespace OpenDDD.Infrastructure.Repository.InMemory.Base
{
    public abstract class InMemoryRepositoryBase<TAggregateRoot, TId> :
        IRepository<TAggregateRoot, TId>
        where TAggregateRoot : AggregateRootBase<TId>
        where TId : notnull
    {
        private readonly ConcurrentDictionary<TId, TAggregateRoot> _store = new();
        private readonly ILogger _logger;

        protected InMemoryRepositoryBase(ILogger logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task<TAggregateRoot> GetAsync(TId id, CancellationToken ct)
        {
            if (_store.TryGetValue(id, out var aggregateRoot))
            {
                // Return a cloned copy of the entity
                return Task.FromResult(Clone(aggregateRoot));
            }

            throw new KeyNotFoundException($"Entity with ID {id} was not found.");
        }

        public Task<TAggregateRoot?> FindAsync(TId id, CancellationToken ct)
        {
            _store.TryGetValue(id, out var aggregateRoot);
            return Task.FromResult(aggregateRoot != null ? Clone(aggregateRoot) : null);
        }

        public Task<IEnumerable<TAggregateRoot>> FindWithAsync(Expression<Func<TAggregateRoot, bool>> filterExpression, CancellationToken ct)
        {
            var filter = filterExpression.Compile();
            var results = _store.Values.Where(filter).Select(Clone);
            return Task.FromResult(results.AsEnumerable());
        }

        public Task<IEnumerable<TAggregateRoot>> FindAllAsync(CancellationToken ct)
        {
            return Task.FromResult(_store.Values.Select(Clone).AsEnumerable());
        }

        public Task SaveAsync(TAggregateRoot aggregateRoot, CancellationToken ct)
        {
            if (aggregateRoot == null) throw new ArgumentNullException(nameof(aggregateRoot));

            _store.AddOrUpdate(aggregateRoot.Id,
                aggregateRoot,
                (_, _) =>
                {
                    _logger.LogInformation($"Updated entity with ID {aggregateRoot.Id}");
                    return aggregateRoot;
                });

            _logger.LogInformation($"Saved entity with ID {aggregateRoot.Id}");
            return Task.CompletedTask;
        }

        public Task DeleteAsync(TAggregateRoot aggregateRoot, CancellationToken ct)
        {
            if (aggregateRoot == null) throw new ArgumentNullException(nameof(aggregateRoot));

            if (_store.TryRemove(aggregateRoot.Id, out _))
            {
                _logger.LogInformation($"Deleted entity with ID {aggregateRoot.Id}");
            }
            else
            {
                _logger.LogWarning($"Failed to delete entity with ID {aggregateRoot.Id}. It may not exist.");
            }

            return Task.CompletedTask;
        }

        private TAggregateRoot Clone(TAggregateRoot source)
        {
            // Use serialization to create a deep copy
            var serialized = JsonConvert.SerializeObject(source);
            return JsonConvert.DeserializeObject<TAggregateRoot>(serialized)!;
        }
    }
}
