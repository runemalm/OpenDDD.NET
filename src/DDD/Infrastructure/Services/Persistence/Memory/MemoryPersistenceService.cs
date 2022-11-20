using System.Threading.Tasks;
using DDD.Application.Settings;
using DDD.Logging;

namespace DDD.Infrastructure.Services.Persistence.Memory
{
	public class MemoryPersistenceService : PersistenceService
	{
		public MemoryPersistenceService(ISettings settings, ILogger logger) 
			: base("n/a", logger)
		{
			
		}

		public override async Task<IConnection> OpenConnectionAsync()
		{
			var conn = new MemoryConnection();
			await conn.Open();
			return conn;
		}
	}
}
