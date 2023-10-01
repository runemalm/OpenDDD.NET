using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace OpenDDD.Infrastructure.Ports.Adapters.Database.Memory
{
    public class MemoryDatabaseConnection : BaseDatabaseConnection<IMemoryDatabaseConnectionSettings>, IDisposable
    {
        private readonly ILogger _logger;
        private readonly IMemoryDatabase _database;
        private bool _isConnected;

        public MemoryDatabaseConnection(ILogger<MemoryDatabaseConnection> logger, IMemoryDatabaseConnectionSettings settings, IMemoryDatabase database) 
            : base(settings)
        {
            _logger = logger;
            _database = database;
        }
        
        // IDisposable

        public override void Dispose()
        {
            Stop(CancellationToken.None);
        }

        // IDatabaseConnection

        public override void Open()
        {
            if (!_isConnected)
            {
                _isConnected = true;
                _logger.LogDebug("Opening memory database connection.");
            }
        }

        public override Task OpenAsync()
        {
            Open();
            return Task.CompletedTask;
        }

        public override void TruncateDatabase()
        {
            AssureConnected();
            _database.Truncate();
        }

        public override async Task TruncateDatabaseAsync()
        {
            await _database.TruncateAsync();
        }

        public override void Close()
        {
            if (_isConnected)
            {
                _isConnected = false;
                _logger.LogDebug("Closing memory database connection.");
            }
        }

        public override Task CloseAsync()
        {
            Close();
            return Task.CompletedTask;
        }

        public override string NextIdentity()
        {
            return Guid.NewGuid().ToString();
        }

        public override Task<string> NextIdentityAsync()
        {
            return Task.FromResult(NextIdentity());
        }

        public override string AddDocument<T>(string collectionName, T document)
        {
            AssureConnected();
            return _database.AddDocument(collectionName, document);
        }

        // IDocumentDatabaseConnection
        
        public override async Task<string> AddDocumentAsync<T>(string collectionName, T document)
        {
            AssureConnected();
            return await _database.AddDocumentAsync(collectionName, document);
        }

        public override void UpsertDocument<T>(string collectionName, string documentId, T document)
        {
            AssureConnected();
            _database.UpsertDocument(collectionName, documentId, document);
        }

        public override async Task UpsertDocumentAsync<T>(string collectionName, string documentId, T document)
        {
            AssureConnected();
            await _database.UpsertDocumentAsync(collectionName, documentId, document);
        }

        public override async Task<IEnumerable<T>> FindAsync<T>(string collectionName, Expression<Func<T, bool>> filter)
        {
            AssureConnected();
            return await _database.FindAsync(collectionName, filter);
        }

        public override IEnumerable<T> GetAll<T>(string collectionName)
        {
            AssureConnected();
            return _database.GetAll<T>(collectionName);
        }

        public override async Task<IEnumerable<T>> GetAllAsync<T>(string collectionName)
        {
            AssureConnected();
            return await _database.GetAllAsync<T>(collectionName);
        }

        // ITransactionalDatabaseConnection
        
        public override async Task StartTransactionAsync()
        {
            if (!_isConnected)
                throw new ApplicationException("Can't start transaction, no connection has been made.");
            await _database.StartTransactionAsync();
        }
        
        public override async Task CommitTransactionAsync()
        {
            if (!_isConnected)
                throw new ApplicationException("Can't commit transaction, no connection has been made.");
            await _database.CommitTransactionAsync();
        }
        
        public override async Task RollbackTransactionAsync()
        {
            if (!_isConnected)
                throw new ApplicationException("Can't rollback transaction, no connection has been made.");
            await _database.RollbackTransactionAsync();
        }
        
        // IStartable
        
        public override bool IsStarted { get; set; }
        public override void Start(CancellationToken ct)
        {
            Open();
            IsStarted = true;
        }

        public override Task StartAsync(CancellationToken ct, bool blocking = false)
        {
            Start(ct);
            return Task.CompletedTask;
        }

        public override void Stop(CancellationToken ct)
        {
            Close();
            IsStarted = false;
        }

        public override Task StopAsync(CancellationToken ct, bool blocking = false)
        {
            Stop(ct);
            return Task.CompletedTask;
        }
        
        // Helpers
        
        public void AssureConnected()
        {
            if (!_isConnected)
                throw new ApplicationException("Can't perform operation, not connected.");
        }
    }
}
