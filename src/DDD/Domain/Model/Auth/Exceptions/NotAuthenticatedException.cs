using System;

namespace DDD.Domain.Model.Auth.Exceptions
{
	public class NotAuthenticatedException : AuthException
    {
        public NotAuthenticatedException()
        {
        }

        public NotAuthenticatedException(string message)
            : base(message)
        {

        }

        public NotAuthenticatedException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
