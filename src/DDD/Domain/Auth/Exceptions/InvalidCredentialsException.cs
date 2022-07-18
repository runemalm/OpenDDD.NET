using System;

namespace DDD.Domain.Auth.Exceptions
{
	public class InvalidCredentialsException : AuthException
    {
        public InvalidCredentialsException()
        {
        }

        public InvalidCredentialsException(string message)
            : base(message)
        {

        }

        public InvalidCredentialsException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
