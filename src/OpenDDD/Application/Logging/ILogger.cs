using System;
using Microsoft.Extensions.Logging;

namespace OpenDDD.Logging
{
	public interface ILogger
	{
		void Log(string message, LogLevel logLevel, Exception exception = null);
	}
}
