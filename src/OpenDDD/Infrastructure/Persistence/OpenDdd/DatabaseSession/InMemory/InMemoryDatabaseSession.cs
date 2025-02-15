using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using OpenDDD.Infrastructure.Persistence.DatabaseSession;
using OpenDDD.Infrastructure.Persistence.Storage;

namespace OpenDDD.Infrastructure.Persistence.OpenDdd.DatabaseSession.InMemory
{
    public class InMemoryDatabaseSession : IDatabaseSession
    {
        private readonly IKeyValueStorage _storage;
        private readonly ILogger<InMemoryDatabaseSession> _logger;
        private readonly ConcurrentDictionary<string, object> _transactionChanges = new();
        private bool _transactionActive = false;

        public InMemoryDatabaseSession(IKeyValueStorage storage, ILogger<InMemoryDatabaseSession> logger)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        private static string GetKey(string collectionName, object id) => $"{collectionName}:{id}";

        public Task OpenConnectionAsync(CancellationToken ct = default) => Task.CompletedTask;

        public Task BeginTransactionAsync(CancellationToken ct = default)
        {
            _transactionChanges.Clear();
            _transactionActive = true;
            _logger.LogDebug("Transaction started.");
            return Task.CompletedTask;
        }

        public async Task CommitTransactionAsync(CancellationToken ct = default)
        {
            if (!_transactionActive)
                throw new InvalidOperationException("No active transaction to commit.");

            foreach (var (key, value) in _transactionChanges)
            {
                await _storage.PutAsync(key, value, ct);
            }

            _transactionChanges.Clear();
            _transactionActive = false;
            _logger.LogDebug("Transaction committed.");
        }

        public Task RollbackTransactionAsync(CancellationToken ct = default)
        {
            if (_transactionActive)
            {
                _transactionChanges.Clear();
                _transactionActive = false;
                _logger.LogDebug("Transaction rolled back.");
            }
            return Task.CompletedTask;
        }

        public Task UpsertAsync<T>(string collectionName, object id, T entity, CancellationToken ct)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            var key = GetKey(collectionName, id);

            if (_transactionActive)
            {
                _transactionChanges[key] = entity;
            }
            else
            {
                return _storage.PutAsync(key, entity, ct);
            }

            _logger.LogDebug($"Upserted entity in collection '{collectionName}' with ID '{id}'");
            return Task.CompletedTask;
        }

        public Task<T?> SelectAsync<T>(string collectionName, object id, CancellationToken ct)
        {
            var key = GetKey(collectionName, id);
            return _storage.GetAsync<T>(key, ct);
        }

        public async Task<IEnumerable<T>> SelectAllAsync<T>(string collectionName, CancellationToken ct)
        {
            var keyPrefix = $"{collectionName}:";
            return await _storage.GetByPrefixAsync<T>(keyPrefix, ct);
        }

        public Task DeleteAsync(string collectionName, object id, CancellationToken ct)
        {
            var key = GetKey(collectionName, id);
            return _storage.RemoveAsync(key, ct);
        }
    }
}
