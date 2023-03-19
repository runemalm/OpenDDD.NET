using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace DDD.Infrastructure.Services.Persistence
{
    public interface IConnection : IDisposable
    {
        Task<int> ExecuteNonQueryAsync(string stmt, IDictionary<string, object> parameters);
        Task<int> ExecuteNonQueryAsync(string stmt);
        Task<IEnumerable<T>> ExecuteQueryAsync<T>(string stmt, IDictionary<string, object> parameters);
        Task<IEnumerable<T>> ExecuteQueryAsync<T>(string stmt);
        Task<T> ExecuteScalarAsync<T>(string stmt, IDictionary<string, object> parameters);

        Task OpenAsync();
        Task CloseAsync();
        Task StartTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
    }
}
