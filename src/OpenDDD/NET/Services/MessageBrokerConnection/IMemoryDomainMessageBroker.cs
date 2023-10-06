using OpenDDD.Infrastructure.Ports.Adapters.MessageBroker.Memory;
using OpenDDD.Infrastructure.Ports.MessageBroker;

namespace OpenDDD.NET.Services.MessageBrokerConnection
{
    public interface IMemoryDomainMessageBroker : IMemoryMessageBroker, IMessageBroker, IPubSubModelMessageBroker
    {
        
    }
}
