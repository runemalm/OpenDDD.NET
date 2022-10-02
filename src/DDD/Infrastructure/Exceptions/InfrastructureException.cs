using System;

namespace DDD.Infrastructure.Exceptions
{
	public class InfrastructureException : Exception
	{
        public InfrastructureException()
        {
        }

        public InfrastructureException(string message)
            : base(message)
        {
            
        }

        public InfrastructureException(string message, Exception inner)
            : base(message, inner)
        {
        }
	}
}
