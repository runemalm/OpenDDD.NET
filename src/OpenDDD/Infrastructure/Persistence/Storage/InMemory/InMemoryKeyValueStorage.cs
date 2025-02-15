using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace OpenDDD.Infrastructure.Persistence.Storage.InMemory
{
    public class InMemoryKeyValueStorage : IKeyValueStorage
    {
        private readonly ILogger<InMemoryKeyValueStorage> _logger;
        private readonly ConcurrentDictionary<string, object> _storage = new();

        public InMemoryKeyValueStorage(ILogger<InMemoryKeyValueStorage> logger)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task PutAsync<T>(string key, T value, CancellationToken ct)
        {
            if (value == null) throw new ArgumentNullException(nameof(value));

            _storage[key] = value;
            _logger.LogDebug("Stored value with key '{Key}'", key);
            return Task.CompletedTask;
        }

        public Task<T?> GetAsync<T>(string key, CancellationToken ct)
        {
            return Task.FromResult(_storage.TryGetValue(key, out var value) ? (T)value : default);
        }

        public Task<IEnumerable<T>> GetByPrefixAsync<T>(string keyPrefix, CancellationToken ct)
        {
            var results = _storage
                .Where(kvp => kvp.Key.StartsWith(keyPrefix))
                .Select(kvp => (T)kvp.Value)
                .ToList();

            return Task.FromResult<IEnumerable<T>>(results);
        }

        public Task RemoveAsync(string key, CancellationToken ct)
        {
            _storage.TryRemove(key, out _);
            _logger.LogDebug("Removed value with key '{Key}'", key);
            return Task.CompletedTask;
        }
    }
}
