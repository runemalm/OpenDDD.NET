using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace OpenDDD.Infrastructure.Ports.Adapters.Database.Memory
{
    public class MemoryDatabase : IMemoryDatabase
    {
        private readonly ILogger _logger;
        private readonly IMemoryDatabaseStore _store;
        private bool _isInTransaction = false;
        
        public MemoryDatabase(ILogger<MemoryDatabase> logger, IMemoryDatabaseStore store)
        {
            _logger = logger;
            _store = store;
        }
        
        // IDatabase

        public void Truncate()
        {
            _store.Truncate();
        }
        
        public Task TruncateAsync()
        {
            throw new NotImplementedException();
        }
        
        // IDocumentDatabase

        public string NextIdentity()
        {
            return Guid.NewGuid().ToString();
        }

        public Task<string> NextIdentityAsync()
        {
            return Task.FromResult(NextIdentity());
        }

        public string AddDocument<T>(string collectionName, T document) where T : class
        {
            var documentId = NextIdentity();
            _store.Upsert(collectionName, documentId, document);
            return documentId;
        }

        public async Task<string> AddDocumentAsync<T>(string collectionName, T document) where T : class
        {
            var documentId = NextIdentity();
            await _store.UpsertAsync(collectionName, documentId, document);
            return documentId;
        }

        public void UpsertDocument<T>(string collectionName, string documentId, T document) where T : class
        {
            _store.Upsert(collectionName, documentId, document);
        }

        public async Task UpsertDocumentAsync<T>(string collectionName, string documentId, T document) where T : class
        {
            await _store.UpsertAsync(collectionName, documentId, document);
        }

        public IEnumerable<T> Find<T>(string collectionName, Expression<Func<T, bool>> filter) where T : class
        {
            return _store.Find(collectionName, filter);
        }

        public async Task<IEnumerable<T>> FindAsync<T>(string collectionName, Expression<Func<T, bool>> filter) where T : class
        {
            return await _store.FindAsync(collectionName, filter);
        }

        public IEnumerable<T> GetAll<T>(string collectionName) where T : class
        {
            return _store.GetAll<T>(collectionName);
        }

        public async Task<IEnumerable<T>> GetAllAsync<T>(string collectionName) where T : class
        {
            return await _store.GetAllAsync<T>(collectionName);
        }

        public T? FindOneAndUpdate<T>(string collectionName, Expression<Func<T, bool>> filter, Action<T> updateAction) where T : class
        {
            throw new NotImplementedException();
            
            // var document = Find(collectionName, filter).SingleOrDefault();
            // if (document != null)
            // {
            //     updateAction(document);
            //     UpsertDocument(collectionName, document., document);
            //     return document;
            // }
            // return null;
        }

        public Task<T?> FindOneAndUpdateAsync<T>(string collectionName, Expression<Func<T, bool>> filter, Action<T> updateAction) where T : class
        {
            throw new NotImplementedException();
        }

        public int GetCount(string collectionName)
        {
            return _store.GetCount(collectionName);
        }

        public async Task<int> GetCountAsync(string collectionName)
        {
            return await _store.GetCountAsync(collectionName);
        }

        // ITransactionalDatabase
        
        public Task StartTransactionAsync()
        {
            if (_isInTransaction)
                throw new ApplicationException("Can't start transaction, already started.");
            _isInTransaction = true;
            _logger.LogDebug("Starting transaction.");
            return Task.CompletedTask;
        }

        public Task CommitTransactionAsync()
        {
            if (!_isInTransaction)
                throw new ApplicationException("Can't commit non-existing transaction.");
            _isInTransaction = false;
            _logger.LogDebug("Committing transaction.");
            return Task.CompletedTask;
        }

        public Task RollbackTransactionAsync()
        {
            if (!_isInTransaction)
                throw new ApplicationException("Can't rollback non-existing transaction.");
            _isInTransaction = false;
            _logger.LogDebug("Rolling back transaction.");
            return Task.CompletedTask;
        }
    }
}
