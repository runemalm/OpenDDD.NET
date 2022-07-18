using System;

namespace DDD.Infrastructure.Ports.Adapters.Postgres.Exceptions
{
	public class PostgresException : Exception
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
