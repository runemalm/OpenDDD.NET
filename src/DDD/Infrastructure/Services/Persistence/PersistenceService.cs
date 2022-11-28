using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using DDD.Application;
using DDD.Application.Exceptions;
using DDD.Domain;
using DDD.Domain.Model;
using DDD.Logging;

namespace DDD.Infrastructure.Services.Persistence
{
	public abstract class PersistenceService : IPersistenceService
	{
		protected readonly ILogger _logger;
		protected readonly string _connString;
		private readonly ConcurrentDictionary<ActionId, IConnection> _connections;

		public PersistenceService(string connString, ILogger logger)
		{
			_logger = logger;
			_connString = connString;
			_connections = new ConcurrentDictionary<ActionId, IConnection>();
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
				var success = _connections.TryAdd(actionId, conn);
				if (!success)
					throw new DddException("Couldn't add connection to collection.");
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
				IConnection removedConn;
				var success = _connections.TryRemove(actionId, out removedConn);
				if (!success)
					throw new DddException("Couldn't remove connection from collection.");
			}
			return Task.CompletedTask;
		}

		public abstract Task<IConnection> OpenConnectionAsync();
	}
}
