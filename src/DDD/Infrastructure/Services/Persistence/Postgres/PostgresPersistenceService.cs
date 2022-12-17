using System.Threading.Tasks;
using DDD.Application.Settings;
using DDD.Logging;
using Npgsql;

namespace DDD.Infrastructure.Services.Persistence.Postgres
{
	public class PostgresPersistenceService : PersistenceService
	{
		public PostgresPersistenceService(ISettings settings, ILogger logger) 
			: base(GetConnString(settings), logger)
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

		public override async Task<IConnection> OpenConnectionAsync()
		{
			var conn = new PostgresConnection(_connString);
			await conn.Open();
			return conn;
		}
	}
}
