using System;
using DDD.Application.Exceptions;

namespace DDD.Application.Settings.Exceptions
{
	public class SettingsException : DddException
	{
        public SettingsException()
        {
        }

        public SettingsException(string message)
            : base(message)
        {
            
        }

        public SettingsException(string message, Exception inner)
            : base(message, inner)
        {
        }
	}
}
