using System;

namespace DDD.Infrastructure.Ports.Adapters.Common.Exceptions
{
	public class MemoryException : Exception
	{
        public MemoryException()
        {
        }

        public MemoryException(string message)
            : base(message)
        {
            
        }

        public MemoryException(string message, Exception inner)
            : base(message, inner)
        {
        }
	}
}
