using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenDDD.Infrastructure.Ports.Adapters.Common.Translation.Converters;

namespace OpenDDD.Infrastructure.Services.Persistence.Memory
{
    public class MemoryConnection : Connection
    {
        private bool _hasConnection;
        private bool _hasTransaction;

        public MemoryConnection(ConversionSettings conversionSettings) : base("n/a", conversionSettings)
        {
            
        }
        public override int ExecuteNonQuery(string stmt, IDictionary<string, object> parameters)
            => 0;

        public override Task<int> ExecuteNonQueryAsync(string stmt, IDictionary<string, object> parameters)
            => Task.FromResult(0);

        public override async Task<IEnumerable<T>> ExecuteQueryAsync<T>(string stmt, IDictionary<string, object> parameters)
        {
            return await Task.FromResult(new List<T>());
        }
        
        public override async Task<T> ExecuteScalarAsync<T>(string stmt, IDictionary<string, object>? parameters)
        {
            throw new NotImplementedException();
        }
        
        public override IEnumerable<T> ExecuteQuery<T>(string stmt, IDictionary<string, object> parameters)
        {
            throw new NotImplementedException();
        }

        public override T ExecuteScalar<T>(string stmt, IDictionary<string, object> parameters)
        {
            throw new NotImplementedException();
        }

        public override Task StartTransactionAsync()
        {
            if (!_hasConnection)
                throw new ApplicationException(
                    "Can't start transaction, no connection has been made.");
            _hasTransaction = true;
            return Task.CompletedTask;
        }
        
        public override async Task CommitTransactionAsync()
        {
            if (!_hasTransaction)
                throw new ApplicationException(
                    "Can't commit non-existing transaction.");
            _hasTransaction = false;
        }
        
        public override async Task RollbackTransactionAsync()
        {
            if (!_hasTransaction)
                throw new ApplicationException(
                    "Can't rollback non-existing transaction.");
            _hasTransaction = false;
        }

        public override void Open()
            => _hasConnection = true;

        public override Task OpenAsync()
        {
            Open();
            return Task.CompletedTask;
        }
        
        public override void Close()
            => _hasConnection = false;

        public override Task CloseAsync()
        {
            Close();
            return Task.CompletedTask;
        }
    }
}
