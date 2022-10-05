using System;
using Microsoft.Extensions.Logging;
using DDD.Application.Settings;
using IMicrosoftLogger = Microsoft.Extensions.Logging.ILogger;
using MicrosoftLogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace DDD.Logging
{
	public class Logger : ILogger
	{
		public string Category;
		private readonly IMicrosoftLogger _microsoftLogger;

		public Logger(ISettings settings, IMicrosoftLogger microsoftLogger)
		{
			_microsoftLogger = microsoftLogger;
			Category = settings.General.Context;
		}

		public void Log(string message, LogLevel logLevel, Exception exception = null)
			=> _microsoftLogger.Log((MicrosoftLogLevel)logLevel, exception, message);
	}
}
