using System;

namespace DDD.Application.Exceptions
{
	public class DddException : Exception
	{
        public DddException()
        {
        }

        public DddException(string message)
            : base(message)
        {
            
        }

        public DddException(string message, Exception inner)
            : base(message, inner)
        {
        }
	}
}
