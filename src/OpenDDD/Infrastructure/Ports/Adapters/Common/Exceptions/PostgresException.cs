using System;

namespace OpenDDD.Infrastructure.Ports.Adapters.Common.Exceptions
{
	public class PostgresException : InfrastructureException
	{
        public PostgresException()
        {
        }

        public PostgresException(string message)
            : base(message)
        {
            
        }

        public PostgresException(string message, Exception inner)
            : base(message, inner)
        {
        }
	}
}
