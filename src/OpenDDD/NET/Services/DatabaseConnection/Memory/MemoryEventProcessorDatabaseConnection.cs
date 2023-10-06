using Microsoft.Extensions.Logging;
using OpenDDD.Infrastructure.Ports.Adapters.Database.Memory;

namespace OpenDDD.NET.Services.DatabaseConnection.Memory
{
    public class MemoryEventProcessorDatabaseConnection : MemoryDatabaseConnection, IEventProcessorDatabaseConnection
    {
        public MemoryEventProcessorDatabaseConnection(ILogger<MemoryEventProcessorDatabaseConnection> logger, IMemoryEventProcessorDatabaseConnectionSettings settings, IMemoryDatabase database) 
            : base(logger, settings, database)
        {
            
        }
    }
}
