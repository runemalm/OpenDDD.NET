using System;

namespace DDD.Domain.Auth.Exceptions
{
	public class ForbiddenException : AuthException
    {
        public ForbiddenException()
        {
        }

        public ForbiddenException(string message)
            : base(message)
        {

        }

        public ForbiddenException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
