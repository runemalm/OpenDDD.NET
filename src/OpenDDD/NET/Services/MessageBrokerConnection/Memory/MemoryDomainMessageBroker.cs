using Microsoft.Extensions.Logging;
using OpenDDD.Infrastructure.Ports.Adapters.MessageBroker.Memory;

namespace OpenDDD.NET.Services.MessageBrokerConnection.Memory
{
    public class MemoryDomainMessageBroker : MemoryMessageBroker, IMemoryDomainMessageBroker
    {
        public MemoryDomainMessageBroker(ILogger<MemoryDomainMessageBroker> logger) 
            : base(logger)
        {
            
        }
    }
}
