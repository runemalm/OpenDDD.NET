using System;

namespace DDD.Domain.Model.Auth.Exceptions
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
