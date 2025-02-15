using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace OpenDDD.Infrastructure.Persistence.Storage.InMemory
{
    public class InMemoryStorage : IStorage
    {
        private readonly ILogger<InMemoryStorage> _logger;
        private readonly ConcurrentDictionary<string, ConcurrentDictionary<object, object>> _database = new();

        public InMemoryStorage(ILogger<InMemoryStorage> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task SaveAsync<T>(string tableName, object id, T entity, CancellationToken ct)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            var table = _database.GetOrAdd(tableName, _ => new ConcurrentDictionary<object, object>());
            table[id] = entity;

            _logger.LogDebug("Saved entity in table '{TableName}' with ID '{Id}'", tableName, id);
            return Task.CompletedTask;
        }

        public Task<T?> LoadAsync<T>(string tableName, object id, CancellationToken ct)
        {
            if (_database.TryGetValue(tableName, out var table) && table.TryGetValue(id, out var entity))
            {
                return Task.FromResult<T?>((T)entity);
            }

            return Task.FromResult<T?>(default);
        }

        public Task<IEnumerable<T>> LoadAllAsync<T>(string tableName, CancellationToken ct)
        {
            if (_database.TryGetValue(tableName, out var table))
            {
                var results = table.Values.Cast<T>().ToList();
                return Task.FromResult<IEnumerable<T>>(results);
            }

            return Task.FromResult<IEnumerable<T>>(Array.Empty<T>());
        }

        public Task DeleteAsync(string tableName, object id, CancellationToken ct)
        {
            if (_database.TryGetValue(tableName, out var table))
            {
                table.TryRemove(id, out _);
            }

            _logger.LogDebug("Deleted entity from table '{TableName}' with ID '{Id}'", tableName, id);
            return Task.CompletedTask;
        }
    }
}
