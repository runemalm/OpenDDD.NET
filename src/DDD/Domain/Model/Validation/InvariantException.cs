using System;
using DDD.Domain.Model.Error;

namespace DDD.Domain.Model.Validation
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
