using Microsoft.Extensions.Logging;
using OpenDDD.Infrastructure.Ports.Adapters.MessageBroker.Memory;

namespace OpenDDD.NET.Services.MessageBrokerConnection.Memory
{
    public class MemoryDomainMessageBrokerConnection : MemoryMessageBrokerConnection, IDomainMessageBrokerConnection
    {
        public MemoryDomainMessageBrokerConnection(ILogger<MemoryDomainMessageBrokerConnection> logger, IMemoryDomainMessageBrokerConnectionSettings settings, IMemoryDomainMessageBroker messageBroker) 
            : base(logger, settings, messageBroker)
        {
            
        }
    }
}
