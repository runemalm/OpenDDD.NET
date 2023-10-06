using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using OpenDDD.Infrastructure.Ports.Database;
using OpenDDD.Infrastructure.Services.Serialization;

namespace OpenDDD.Infrastructure.Ports.Adapters.Database.Memory
{
    public class MemoryDatabaseStore : IMemoryDatabaseStore, IDatabaseStore, IDocumentDatabaseStore
    {
        private readonly ISerializer _serializer;
        private IDictionary<string, IDictionary<string, string>> _data;

        public MemoryDatabaseStore(ISerializer serializer)
        {
            _serializer = serializer;
            _data = new Dictionary<string, IDictionary<string, string>>();
        }
        
        // IDatabaseStore

        public void Truncate()
        {
            _data = new Dictionary<string, IDictionary<string, string>>();
        }
        
        // IDocumentDatabaseStore


        public void Upsert<T>(string collectionName, string documentId, T document) where T : class
        {
            AssureCollection(collectionName);
            GetCollection(collectionName)[documentId] = _serializer.Serialize(document);
        }

        public Task UpsertAsync<T>(string collectionName, string documentId, T document) where T : class
        {
            Upsert(collectionName, documentId, document);
            return Task.CompletedTask;
        }

        public IEnumerable<T> GetAll<T>(string collectionName) where T : class
        {
            AssureCollection(collectionName);
            return GetCollection(collectionName).Values.Select(s => (T)_serializer.Deserialize(s));
        }

        public Task<IEnumerable<T>> GetAllAsync<T>(string collectionName) where T : class
        {
            return Task.FromResult(GetAll<T>(collectionName));
        }

        public IEnumerable<T> Find<T>(string collectionName, Expression<Func<T, bool>> filter) where T : class
        {
            AssureCollection(collectionName);
            return GetAll<T>(collectionName).AsEnumerable().Where(filter.Compile());
        }

        public Task<IEnumerable<T>> FindAsync<T>(string collectionName, Expression<Func<T, bool>> filter) where T : class
        {
            return Task.FromResult(Find<T>(collectionName, filter));
        }

        public int GetCount(string collectionName)
        {
            AssureCollection(collectionName);
            return GetCollection(collectionName).Count;
        }

        public Task<int> GetCountAsync(string collectionName)
        {
            return Task.FromResult(GetCount(collectionName));
        }

        // Private
        
        private void AssureCollection(string collectionName)
        {
            if (!_data.ContainsKey(collectionName))
            {
                _data[collectionName] = new Dictionary<string, string>();
            }
        }
        
        private IDictionary<string, string> GetCollection(string collectionName)
        {
            AssureCollection(collectionName);
            return _data[collectionName];
        }
    }
}
