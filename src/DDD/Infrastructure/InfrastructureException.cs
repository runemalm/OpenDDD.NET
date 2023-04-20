using System;
using DDD.Application.Error;

namespace DDD.Infrastructure
{
	public class InfrastructureException : DddException
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
