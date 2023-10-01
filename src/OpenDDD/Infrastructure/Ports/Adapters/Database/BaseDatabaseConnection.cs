using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using OpenDDD.Infrastructure.Ports.Database;

namespace OpenDDD.Infrastructure.Ports.Adapters.Database
{
    public abstract class BaseDatabaseConnection<TSettings> : IDatabaseConnection, IDocumentDatabaseConnection, ITransactionalDatabaseConnection 
        where TSettings : IDatabaseConnectionSettings
    {
        public IDatabaseConnectionSettings Settings { get; set; }
        
        public BaseDatabaseConnection(TSettings settings)
        {
            Settings = settings;
        }
        
        // IDatabaseConnection

        public abstract void Open();
        public abstract Task OpenAsync();
        public abstract void TruncateDatabase();
        public abstract Task TruncateDatabaseAsync();
        public abstract void Close();
        public abstract Task CloseAsync();
        
        // IDocumentDatabaseConnection

        public abstract string NextIdentity();
        public abstract Task<string> NextIdentityAsync();
        public abstract string AddDocument<T>(string collectionName, T document) where T : class;
        public abstract Task<string> AddDocumentAsync<T>(string collectionName, T document) where T : class;
        public abstract void UpsertDocument<T>(string collectionName, string documentId, T document) where T : class;
        public abstract Task UpsertDocumentAsync<T>(string collectionName, string documentId, T document) where T : class;
        public abstract Task<IEnumerable<T>> FindAsync<T>(string collectionName, Expression<Func<T, bool>> filter) where T : class;
        public abstract IEnumerable<T> GetAll<T>(string collectionName) where T : class;
        public abstract Task<IEnumerable<T>> GetAllAsync<T>(string collectionName) where T : class;

        // ITransactionalDatabaseConnection
        
        public abstract Task StartTransactionAsync();
        public abstract Task CommitTransactionAsync();
        public abstract Task RollbackTransactionAsync();

        // IStartable
        
        public abstract bool IsStarted { get; set; }
        public abstract void Start(CancellationToken ct);
        public abstract Task StartAsync(CancellationToken ct, bool blocking = false);
        public abstract void Stop(CancellationToken ct);
        public abstract Task StopAsync(CancellationToken ct, bool blocking = false);
        
        // IDisposable
        public abstract void Dispose();
    }
}
