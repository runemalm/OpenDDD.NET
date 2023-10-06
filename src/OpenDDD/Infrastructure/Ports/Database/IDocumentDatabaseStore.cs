using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace OpenDDD.Infrastructure.Ports.Database
{
    public interface IDocumentDatabaseStore
    {
        void Upsert<T>(string collectionName, string documentId, T document) where T : class;
        Task UpsertAsync<T>(string collectionName, string documentId, T document) where T : class;
        IEnumerable<T> GetAll<T>(string collectionName) where T : class;
        Task<IEnumerable<T>> GetAllAsync<T>(string collectionName) where T : class;
        IEnumerable<T> Find<T>(string collectionName, Expression<Func<T, bool>> filter) where T : class;
        Task<IEnumerable<T>> FindAsync<T>(string collectionName, Expression<Func<T, bool>> filter) where T : class;
        int GetCount(string collectionName);
        Task<int> GetCountAsync(string collectionName);
    }
}
