using System;
using Microsoft.Extensions.Logging;

namespace DDD.Logging
{
	public interface ILogger
	{
		void Log(string message, LogLevel logLevel, Exception exception = null);
	}
}
