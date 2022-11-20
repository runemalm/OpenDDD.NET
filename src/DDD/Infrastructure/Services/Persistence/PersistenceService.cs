using System.Collections.Generic;
using System.Threading.Tasks;
using DDD.Application;
using DDD.Domain;
using DDD.Domain.Model;
using DDD.Logging;

namespace DDD.Infrastructure.Services.Persistence
{
	public abstract class PersistenceService : IPersistenceService
	{
		protected readonly ILogger _logger;
		protected readonly string _connString;
		private readonly IDictionary<ActionId, IConnection> _connections;

		public PersistenceService(string connString, ILogger logger)
		{
			_logger = logger;
			_connString = connString;
			_connections = new Dictionary<ActionId, IConnection>();
		}
		
		private IConnection GetExistingConnectionAsync(ActionId actionId)
		{
			foreach (KeyValuePair<ActionId, IConnection> kvp in _connections)
				if (kvp.Key == actionId)
					return kvp.Value;
			return null;
		}

		public async Task StartTransactionAsync(ActionId actionId)
		{
			var conn = await GetConnectionAsync(actionId);
			await conn.StartTransactionAsync();
		}

		public async Task CommitTransactionAsync(ActionId actionId)
		{
			var conn = await GetConnectionAsync(actionId);
			await conn.CommitTransactionAsync();
		}

		public async Task RollbackTransactionAsync(ActionId actionId)
		{
			var conn = await GetConnectionAsync(actionId);
			await conn.RollbackTransactionAsync();
		}

		public async Task<IConnection> GetConnectionAsync(ActionId actionId)
		{
			var conn = GetExistingConnectionAsync(actionId);
			if (conn == null)
			{
				conn = await OpenConnectionAsync();
				_connections.Add(actionId, conn);
			}
			return conn;
		}
		
		public Task ReleaseConnectionAsync(ActionId actionId)
		{
			var conn = GetExistingConnectionAsync(actionId);
			if (conn == null)
				_logger.Log(
					$"Can't release connection for action ID: {actionId}. None exists.", 
					LogLevel.Warning);
			else
			{
				conn.Close();
				_connections.Remove(actionId);
			}
			return Task.CompletedTask;
		}

		public abstract Task<IConnection> OpenConnectionAsync();
	}
}
