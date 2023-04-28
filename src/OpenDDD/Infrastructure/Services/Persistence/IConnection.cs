using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OpenDDD.Infrastructure.Services.Persistence
{
    public interface IConnection : IDisposable
    {
        void Open();
        Task OpenAsync();
        void Close();
        Task CloseAsync();
        Task StartTransactionAsync();
        Task CommitTransactionAsync();
        Task RollbackTransactionAsync();
        int ExecuteNonQuery(string stmt, IDictionary<string, object> parameters);
        Task<int> ExecuteNonQueryAsync(string stmt, IDictionary<string, object> parameters);
        int ExecuteNonQuery(string stmt);
        Task<int> ExecuteNonQueryAsync(string stmt);
        IEnumerable<T> ExecuteQuery<T>(string stmt, IDictionary<string, object> parameters);
        Task<IEnumerable<T>> ExecuteQueryAsync<T>(string stmt, IDictionary<string, object> parameters);
        IEnumerable<T> ExecuteQuery<T>(string stmt);
        Task<IEnumerable<T>> ExecuteQueryAsync<T>(string stmt);
        T ExecuteScalar<T>(string stmt, IDictionary<string, object> parameters);
        Task<T> ExecuteScalarAsync<T>(string stmt, IDictionary<string, object> parameters);
    }
}
