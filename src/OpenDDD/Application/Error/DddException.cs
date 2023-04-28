using System;

namespace OpenDDD.Application.Error
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
