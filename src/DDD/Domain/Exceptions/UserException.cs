using System;

namespace DDD.Domain.Exceptions
{
	public class UserException : DomainException
    {
        public UserException()
        {
        }

        public UserException(string message)
            : base(message)
        {
            
        }

        public UserException(string message, Exception inner)
            : base(message, inner)
        {
        }
	}
}
