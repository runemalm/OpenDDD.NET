using System.Threading.Tasks;
using DDD.Application.Settings;
using DDD.Logging;

namespace DDD.Infrastructure.Persistence.Postgres
{
	public class PostgresPersistenceService : PersistenceService
	{
		public PostgresPersistenceService(ISettings settings, ILogger logger) 
			: base(settings.Postgres.ConnString, logger)
		{
			
		}
		
		public override async Task<IConnection> OpenConnectionAsync()
		{
			var conn = new PostgresConnection(_connString);
			await conn.Open();
			return conn;
		}
	}
}
