using System.Threading.Tasks;
using OpenDDD.Application.Settings;
using OpenDDD.Infrastructure.Ports.Adapters.Common.Translation.Converters;
using OpenDDD.Logging;

namespace OpenDDD.Infrastructure.Services.Persistence.Memory
{
	public class MemoryPersistenceService : PersistenceService
	{
		public MemoryPersistenceService(ISettings settings, ILogger logger, ConversionSettings conversionSettings) 
			: base("n/a", logger, conversionSettings)
		{
			
		}
		
		public override IConnection OpenConnection()
		{
			var conn = new MemoryConnection(_conversionSettings);
			conn.Open();
			return conn;
		}
		
		public override async Task<IConnection> OpenConnectionAsync()
		{
			var conn = new MemoryConnection(_conversionSettings);
			await conn.OpenAsync();
			return conn;
		}
		
		public override async Task StartAsync()
			=> await base.StartAsync();

		public override async Task StopAsync()
			=> await base.StopAsync();

		public override void EnsureDatabase()
		{
			
		}

		public override Task EnsureDatabaseAsync()
			=> Task.CompletedTask;
	}
}
