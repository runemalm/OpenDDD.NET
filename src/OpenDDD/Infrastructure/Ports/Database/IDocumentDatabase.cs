using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace OpenDDD.Infrastructure.Ports.Database
{
    public interface IDocumentDatabase
    {
        string NextIdentity();
        Task<string> NextIdentityAsync();
        string AddDocument<T>(string collectionName, T document) where T : class;
        Task<string> AddDocumentAsync<T>(string collectionName, T document) where T : class;
        void UpsertDocument<T>(string collectionName, string documentId, T document) where T : class;
        Task UpsertDocumentAsync<T>(string collectionName, string documentId, T document) where T : class;
        IEnumerable<T> Find<T>(string collectionName, Expression<Func<T, bool>> filter) where T : class;
        Task<IEnumerable<T>> FindAsync<T>(string collectionName, Expression<Func<T, bool>> filter) where T : class;
        IEnumerable<T> GetAll<T>(string collectionName) where T : class;
        Task<IEnumerable<T>> GetAllAsync<T>(string collectionName) where T : class;
    }
}
