using Microsoft.Extensions.Logging;
using OpenDDD.Infrastructure.Ports.Adapters.Database.Memory;

namespace OpenDDD.NET.Services.DatabaseConnection.Memory
{
    public class MemoryActionDatabaseConnection : MemoryDatabaseConnection, IActionDatabaseConnection
    {
        public MemoryActionDatabaseConnection(ILogger<MemoryActionDatabaseConnection> logger, IMemoryActionDatabaseConnectionSettings settings, IMemoryDatabase database) 
            : base(logger, settings, database)
        {
            
        }
    }
}
