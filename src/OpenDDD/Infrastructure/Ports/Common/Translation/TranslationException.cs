using System;

namespace OpenDDD.Infrastructure.Ports.Common.Translation
{
	public class TranslationException : InfrastructureException
    {
        public TranslationException()
        {
        }

        public TranslationException(string message)
            : base(message)
        {
            
        }

        public TranslationException(string message, Exception inner)
            : base(message, inner)
        {
        }
	}
}
