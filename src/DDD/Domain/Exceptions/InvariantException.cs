using System;

namespace DDD.Domain.Exceptions
{
	public class InvariantException : DomainException
    {
        public InvariantException()
        {
        }

        public InvariantException(string message)
            : base(message)
        {
            
        }

        public InvariantException(string message, Exception inner)
            : base(message, inner)
        {
        }
	}
}
