using System;
using OpenDDD.Application.Error;

namespace OpenDDD.Infrastructure
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
