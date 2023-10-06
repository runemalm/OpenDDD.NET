using System;
using System.Threading.Tasks;
using OpenDDD.NET;

namespace OpenDDD.Infrastructure.Ports.MessageBroker
{
    public interface IMessageBrokerConnection : IPubSubModelMessageBrokerConnection, IStartable, IDisposable
    {
        IMessageBrokerConnectionSettings Settings { get; set; }
        
        void Open();
        Task OpenAsync();
        void Close();
        Task CloseAsync();
    }
}
