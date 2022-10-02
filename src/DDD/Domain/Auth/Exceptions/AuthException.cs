using System;
using DDD.Application.Exceptions;

namespace DDD.Domain.Auth.Exceptions
{
	public class AuthException : DddException
	{
        public AuthException()
        {
        }

        public AuthException(string message)
            : base(message)
        {

        }

        public AuthException(string message, Exception inner)
            : base(message, inner)
        {
        }
    }
}
