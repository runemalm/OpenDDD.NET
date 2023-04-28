using System;

namespace OpenDDD.Infrastructure.Ports.PubSub
{
	public class ListenerException : InfrastructureException
    {
        public ListenerException()
        {
        }

        public ListenerException(string message)
            : base(message)
        {
            
        }

        public ListenerException(string message, Exception inner)
            : base(message, inner)
        {
        }
	}
}
