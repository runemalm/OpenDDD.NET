using System.Collections.Generic;
using System.Threading.Tasks;
using DDD.Application.Exceptions;

namespace DDD.Infrastructure.Persistence.Memory
{
    public class MemoryConnection : Connection
    {
        private bool _hasConnection;
        private bool _hasTransaction;

        public MemoryConnection() : base("n/a")
        {
            
        }

        public override async Task Open()
        {
            _hasConnection = true;
        }
        
        public override async Task Close()
        {
            _hasConnection = false;
        }

        public override Task StartTransactionAsync()
        {
            if (!_hasConnection)
                throw new DddException(
                    "Can't start transaction, no connection has been made.");
            _hasTransaction = true;
            return Task.CompletedTask;
        }
        
        public override async Task CommitTransactionAsync()
        {
            if (!_hasTransaction)
                throw new DddException(
                    "Can't commit non-existing transaction.");
            _hasTransaction = false;
        }
        
        public override async Task RollbackTransactionAsync()
        {
            if (!_hasTransaction)
                throw new DddException(
                    "Can't rollback non-existing transaction.");
            _hasTransaction = false;
        }
        
        public override async Task<int> ExecuteNonQueryAsync(string stmt, IDictionary<string, object> parameters)
        {
            return await Task.FromResult(0);
        }

        public override async Task<IEnumerable<T>> ExecuteQueryAsync<T>(string stmt, IDictionary<string, object> parameters)
        {
            return await Task.FromResult(new List<T>());
        }
    }
}
