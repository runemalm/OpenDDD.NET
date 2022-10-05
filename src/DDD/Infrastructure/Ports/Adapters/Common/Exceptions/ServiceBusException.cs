using System;

namespace DDD.Infrastructure.Ports.Adapters.Common.Exceptions
{
	public class ServiceBusException : Exception
	{
        public ServiceBusException()
        {
        }

        public ServiceBusException(string message)
            : base(message)
        {
            
        }

        public ServiceBusException(string message, Exception inner)
            : base(message, inner)
        {
        }
	}
}
