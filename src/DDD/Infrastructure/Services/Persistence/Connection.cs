using System.Collections.Generic;
using System.Threading.Tasks;

namespace DDD.Infrastructure.Services.Persistence
{
	public abstract class Connection : IConnection
	{
		protected readonly string _connString;

		public Connection(string connString)
		{
			_connString = connString;
		}

		public abstract Task Open();
		public abstract Task Close();
		public abstract Task StartTransactionAsync();
		public abstract Task CommitTransactionAsync();
		public abstract Task RollbackTransactionAsync();
		public abstract Task<int> ExecuteNonQueryAsync(string stmt, IDictionary<string, object> parameters);
		public Task<int> ExecuteNonQueryAsync(string stmt)
			=> ExecuteNonQueryAsync(stmt, null);
		public abstract Task<IEnumerable<T>> ExecuteQueryAsync<T>(string stmt, IDictionary<string, object> parameters);
		public Task<IEnumerable<T>> ExecuteQueryAsync<T>(string stmt)
			=> ExecuteQueryAsync<T>(stmt, null);
		public abstract Task<T> ExecuteScalarAsync<T>(string stmt, IDictionary<string, object> parameters);

		public void Dispose()
		{
			Close();
		}
	}
}
