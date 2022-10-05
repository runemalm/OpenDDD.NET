using System;

namespace DDD.Domain.Model.Auth.Exceptions
{
	public class MissingCredentialsException : AuthException
    {
        public MissingCredentialsException()
        {
        }

        public MissingCredentialsException(string message)
            : base(message)
        {

        }

        public MissingCredentialsException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
