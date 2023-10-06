using System;
using System.Threading.Tasks;

namespace OpenDDD.Infrastructure.Ports.MessageBroker
{
    public interface ISubscription
    {
        Topic Topic { get; set; }
        ConsumerGroup ConsumerGroup { get; set; }
        
        // Events
        
        event ReceivedEventHandlerAsync Received;
        delegate Task ReceivedEventHandlerAsync(object sender, ReceivedEventArgs e);
        Task OnReceivedAsync(ReceivedEventArgs e);

        public class ReceivedEventArgs : EventArgs
        {
            public IMessage Message { get; }

            public ReceivedEventArgs(IMessage message)
            {
                Message = message;
            }
        }
    }
}
