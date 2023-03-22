using System;
using DDD.Application.Error;

namespace DDD.Infrastructure.Ports.Adapters.Common.Exceptions
{
	public class PostgresException : DddException
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
