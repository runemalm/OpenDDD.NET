using System;

namespace OpenDDD.Infrastructure.Ports.Adapters.Common.Exceptions
{
	public class RabbitException : Exception
	{
        public RabbitException()
        {
        }

        public RabbitException(string message)
            : base(message)
        {
            
        }

        public RabbitException(string message, Exception inner)
            : base(message, inner)
        {
        }
	}
}
