using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using OpenDDD.Infrastructure.Persistence.DatabaseSession;
using OpenDDD.Infrastructure.Persistence.Storage;

namespace OpenDDD.Infrastructure.Persistence.OpenDdd.DatabaseSession.InMemory
{
    public class InMemoryDatabaseSession : IDatabaseSession
    {
        private readonly IStorage _storage;
        private readonly ILogger<InMemoryDatabaseSession> _logger;
        private readonly ConcurrentDictionary<string, List<(object Id, object Entity)>> _transactionChanges = new();

        private bool _transactionActive = false;

        public InMemoryDatabaseSession(IStorage storage, ILogger<InMemoryDatabaseSession> logger)
        {
            _storage = storage ?? throw new ArgumentNullException(nameof(storage));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public Task OpenConnectionAsync(CancellationToken ct = default)
        {
            return Task.CompletedTask; // No-op for in-memory
        }

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

            foreach (var (table, changes) in _transactionChanges)
            {
                foreach (var (id, entity) in changes)
                {
                    await _storage.SaveAsync(table, id, entity, ct);
                }
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

        public Task SaveAsync<T>(string tableName, object id, T entity, CancellationToken ct)
        {
            if (entity == null) throw new ArgumentNullException(nameof(entity));

            if (_transactionActive)
            {
                _transactionChanges
                    .GetOrAdd(tableName, _ => new List<(object, object)>())
                    .Add((id, entity));
            }
            else
            {
                return _storage.SaveAsync(tableName, id, entity, ct);
            }

            return Task.CompletedTask;
        }

        public Task<T?> LoadAsync<T>(string tableName, object id, CancellationToken ct)
        {
            return _storage.LoadAsync<T>(tableName, id, ct);
        }

        public Task<IEnumerable<T>> LoadAllAsync<T>(string tableName, CancellationToken ct)
        {
            return _storage.LoadAllAsync<T>(tableName, ct);
        }

        public Task DeleteAsync(string tableName, object id, CancellationToken ct)
        {
            return _storage.DeleteAsync(tableName, id, ct);
        }
    }
}
