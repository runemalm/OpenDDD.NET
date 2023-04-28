using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace OpenDDD.Infrastructure.Services.Persistence
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

		public abstract void Open();
		public abstract Task OpenAsync();
		public abstract void Close();
		public abstract Task CloseAsync();
		public abstract Task StartTransactionAsync();
		public abstract Task CommitTransactionAsync();
		public abstract Task RollbackTransactionAsync();
		public abstract int ExecuteNonQuery(string stmt, IDictionary<string, object> parameters);
		public abstract Task<int> ExecuteNonQueryAsync(string stmt, IDictionary<string, object> parameters);
		public int ExecuteNonQuery(string stmt)
			=> ExecuteNonQuery(stmt, null);
		public Task<int> ExecuteNonQueryAsync(string stmt)
			=> ExecuteNonQueryAsync(stmt, null);
		public abstract IEnumerable<T> ExecuteQuery<T>(string stmt, IDictionary<string, object> parameters);
		public abstract Task<IEnumerable<T>> ExecuteQueryAsync<T>(string stmt, IDictionary<string, object> parameters);
		public IEnumerable<T> ExecuteQuery<T>(string stmt)
			=> ExecuteQuery<T>(stmt, null);
		public Task<IEnumerable<T>> ExecuteQueryAsync<T>(string stmt)
			=> ExecuteQueryAsync<T>(stmt, null);
		public abstract T ExecuteScalar<T>(string stmt, IDictionary<string, object> parameters);
		public abstract Task<T> ExecuteScalarAsync<T>(string stmt, IDictionary<string, object>? parameters);
		
		public void Dispose()
			=> Close();
	}
}
