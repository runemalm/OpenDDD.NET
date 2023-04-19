using System;
using System.Threading.Tasks;
using Npgsql;
using DDD.Application.Settings;
using DDD.Infrastructure.Ports.Adapters.Common.Translation.Converters;
using DDD.Logging;
using PostgresException = DDD.Infrastructure.Ports.Adapters.Common.Exceptions.PostgresException;

namespace DDD.Infrastructure.Services.Persistence.Postgres
{
	public class PostgresPersistenceService : PersistenceService
	{
		public PostgresPersistenceService(ISettings settings, ILogger logger, SerializerSettings serializerSettings) 
			: base(GetConnString(settings), logger, serializerSettings)
		{
			
		}
		
		private static string GetConnString(ISettings settings)
		{
			var connString = settings.Postgres.ConnString;

			var builder = new NpgsqlConnectionStringBuilder(connString);

			builder.Pooling = settings.Persistence.Pooling.Enabled;
			builder.MinPoolSize = settings.Persistence.Pooling.MinSize;
			builder.MaxPoolSize = settings.Persistence.Pooling.MaxSize;

			return builder.ConnectionString;
		}
		
		public override async Task StartAsync()
			=> await base.StartAsync();

		public override async Task StopAsync()
			=> await base.StopAsync();

		public override void EnsureDatabase()
		{
			var builder = new NpgsqlConnectionStringBuilder(_connString);
			var dbName = builder.Database;
			
			if (dbName == null)
				throw new PostgresException("Can't create database. The database was not specified in settings.");

			builder.Database = "postgres";
			using (var conn = new PostgresConnection(builder.ConnectionString, _serializerSettings))
			{
				conn.Open();
				
				var stmt = $"CREATE DATABASE \"{dbName}\"";

				try
				{
					conn.ExecuteNonQuery(stmt);
				}
				catch (Exception e)
				{
					if (!e.Message.Contains("already exists"))
						throw;
				}
			}
		}

		public override async Task EnsureDatabaseAsync()
		{
			var builder = new NpgsqlConnectionStringBuilder(_connString);
			var dbName = builder.Database;
			
			if (dbName == null)
				throw new PostgresException("Can't create database. The database was not specified in settings.");

			builder.Database = "postgres";
			using (var conn = new PostgresConnection(builder.ConnectionString, _serializerSettings))
			{
				await conn.OpenAsync();
				
				var stmt = $"CREATE DATABASE \"{dbName}\"";

				try
				{
					await conn.ExecuteNonQueryAsync(stmt);
				}
				catch (Exception e)
				{
					if (!e.Message.Contains("already exists"))
						throw;
				}
			}
		}

		public override IConnection OpenConnection()
		{
			var conn = new PostgresConnection(_connString, _serializerSettings);
			conn.Open();
			return conn;
		}
		
		public override async Task<IConnection> OpenConnectionAsync()
		{
			var conn = new PostgresConnection(_connString, _serializerSettings);
			await conn.OpenAsync();
			return conn;
		}
	}
}
