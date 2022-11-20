using System;

namespace DDD.Infrastructure
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
