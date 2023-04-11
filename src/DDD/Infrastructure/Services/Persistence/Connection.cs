using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DDD.Infrastructure.Services.Persistence
{
	public abstract class Connection : IConnection
	{
		protected readonly string _connString;
		protected readonly JsonSerializerSettings _serializerSettings;

		public Connection(string connString, JsonSerializerSettings serializerSettings)
		{
			_connString = connString;
			_serializerSettings = serializerSettings;
		}

		public abstract Task OpenAsync();
		public abstract Task CloseAsync();
		public abstract Task StartTransactionAsync();
		public abstract Task CommitTransactionAsync();
		public abstract Task RollbackTransactionAsync();
		public abstract Task<int> ExecuteNonQueryAsync(string stmt, IDictionary<string, object> parameters);
		public Task<int> ExecuteNonQueryAsync(string stmt)
			=> ExecuteNonQueryAsync(stmt, null);
		public abstract Task<IEnumerable<T>> ExecuteQueryAsync<T>(string stmt, IDictionary<string, object> parameters);
		public Task<IEnumerable<T>> ExecuteQueryAsync<T>(string stmt)
			=> ExecuteQueryAsync<T>(stmt, null);
		public abstract Task<T> ExecuteScalarAsync<T>(string stmt, IDictionary<string, object>? parameters);

		public void Dispose()
		{
			CloseAsync().GetAwaiter().GetResult();
		}
	}
}
