using System;

namespace DDD.Infrastructure.Ports.PubSub
{
	public class PubSubException : InfrastructureException
    {
        public PubSubException()
        {
        }

        public PubSubException(string message)
            : base(message)
        {
            
        }

        public PubSubException(string message, Exception inner)
            : base(message, inner)
        {
        }
	}
}
