using System;
using System.Threading.Tasks;
using Npgsql;
using OpenDDD.Application.Settings;
using OpenDDD.Infrastructure.Ports.Adapters.Common.Translation.Converters;
using OpenDDD.Logging;

namespace OpenDDD.Infrastructure.Services.Persistence.Postgres
{
	public class PostgresPersistenceService : PersistenceService
	{
		public PostgresPersistenceService(ISettings settings, ILogger logger, ConversionSettings conversionSettings) 
			: base(GetConnString(settings), logger, conversionSettings)
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
				throw new Ports.Adapters.Common.Exceptions.PostgresException("Can't create database. The database was not specified in settings.");

			builder.Database = "postgres";
			using (var conn = new PostgresConnection(builder.ConnectionString, _conversionSettings))
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
				throw new Ports.Adapters.Common.Exceptions.PostgresException("Can't create database. The database was not specified in settings.");

			builder.Database = "postgres";
			using (var conn = new PostgresConnection(builder.ConnectionString, _conversionSettings))
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
			var conn = new PostgresConnection(_connString, _conversionSettings);
			conn.Open();
			return conn;
		}
		
		public override async Task<IConnection> OpenConnectionAsync()
		{
			var conn = new PostgresConnection(_connString, _conversionSettings);
			await conn.OpenAsync();
			return conn;
		}
	}
}
