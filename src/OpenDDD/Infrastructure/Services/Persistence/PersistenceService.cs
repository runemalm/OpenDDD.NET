using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;
using OpenDDD.Application;
using OpenDDD.Application.Error;
using OpenDDD.Infrastructure.Ports.Adapters.Common.Translation.Converters;
using OpenDDD.Logging;

namespace OpenDDD.Infrastructure.Services.Persistence
{
	public abstract class PersistenceService : IPersistenceService
	{
		protected readonly ILogger _logger;
		protected readonly string _connString;
		protected readonly SerializerSettings _serializerSettings;
		private readonly ConcurrentDictionary<ActionId, IConnection> _connections;

		public PersistenceService(string connString, ILogger logger, SerializerSettings serializerSettings)
		{
			_logger = logger;
			_connString = connString;
			_serializerSettings = serializerSettings;
			_connections = new ConcurrentDictionary<ActionId, IConnection>();
		}
		
		public virtual void Start()
			=> EnsureDatabase();

		public virtual Task StartAsync()
			=> EnsureDatabaseAsync();

		public virtual void Stop()
			=> ReleaseConnection(ActionId.BootId());

		public virtual Task StopAsync()
			=> ReleaseConnectionAsync(ActionId.BootId());

		public abstract void EnsureDatabase();
		public abstract Task EnsureDatabaseAsync();
		
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
		
		public IConnection GetConnection(ActionId actionId)
		{
			var conn = GetExistingConnection(actionId);
			if (conn == null)
			{
				conn = OpenConnection();
				var success = _connections.TryAdd(actionId, conn);
				if (!success)
					throw new DddException("Couldn't add connection to collection.");
			}
			return conn;
		}

		public async Task<IConnection> GetConnectionAsync(ActionId actionId)
		{
			var conn = GetExistingConnection(actionId);
			if (conn == null)
			{
				conn = await OpenConnectionAsync();
				var success = _connections.TryAdd(actionId, conn);
				if (!success)
					throw new DddException("Couldn't add connection to collection.");
			}
			return conn;
		}
		
		public void ReleaseConnection(ActionId actionId)
		{
			var conn = GetExistingConnection(actionId);
			if (conn == null)
				_logger.Log(
					$"Can't release connection for action ID: {actionId}. None exists.", 
					LogLevel.Warning);
			else
			{
				conn.Close();
				var success = _connections.TryRemove(actionId, out _);
				if (!success)
					throw new DddException("Couldn't remove connection from collection.");
			}
		}

		public async Task ReleaseConnectionAsync(ActionId actionId)
		{
			var conn = GetExistingConnection(actionId);
			if (conn == null)
				_logger.Log(
					$"Can't release connection for action ID: {actionId}. None exists.", 
					LogLevel.Warning);
			else
			{
				await conn.CloseAsync();
				var success = _connections.TryRemove(actionId, out _);
				if (!success)
					throw new DddException("Couldn't remove connection from collection.");
			}
		}

		public abstract IConnection OpenConnection();
		public abstract Task<IConnection> OpenConnectionAsync();
		private IConnection GetExistingConnection(ActionId actionId)
		{
			foreach (KeyValuePair<ActionId, IConnection> kvp in _connections)
				if (kvp.Key == actionId)
					return kvp.Value;
			return null;
		}
	}
}
