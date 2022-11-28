using System;

namespace DDD.Domain.Model.Auth.Exceptions
{
	public class MissingJwtTokenException : AuthException
    {
        public MissingJwtTokenException()
        {
        }

        public MissingJwtTokenException(string message)
            : base(message)
        {

        }

        public MissingJwtTokenException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
