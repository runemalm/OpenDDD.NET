using System;
using System.Linq;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.ApplicationInsights;
using DDD.Application.Settings;
using DDD.Logging;
using DDD.Application.Exceptions;
using MsLogLevel = Microsoft.Extensions.Logging.LogLevel;
using LogLevel = DDD.Logging.LogLevel;
using DddSettings = DDD.Application.Settings.Settings;

namespace DDD.NETCore.Extensions
{
	public static class WebHostBuilderExtensions
	{
		// Public API
		
		public static IWebHostBuilder AddEnvFile(this IWebHostBuilder webHostBuilder, string envFileVariableName, string prefix = "", string defaultEnvFileVariableValue = "", bool overwriteExisting = true)
		{
			return webHostBuilder.ConfigureAppConfiguration((context, config) =>
			{
				var builtConfig = config.Build();
				new EnvFileLoader(envFileVariableName, builtConfig)
					.Load(defaultEnvFileVariableValue, overwriteExisting);
				config.AddEnvironmentVariables(prefix: prefix);
			});
		}

		public static IWebHostBuilder AddSettings(this IWebHostBuilder webHostBuilder)
		{
			webHostBuilder =
				webHostBuilder.ConfigureServices((context, services) =>
				{
					// Set server urls
					var urls = context.Configuration.GetSection("HTTP_URLS").Value.Split(',');
					webHostBuilder.UseSetting(WebHostDefaults.ServerUrlsKey, string.Join(';', urls));
					
					// Add settings
					services.Configure<Options>(context.Configuration);
					services.AddTransient<ISettings, DddSettings>();
					
					// TODO
					services.AddSingleton<IRegistrationTracker, RegistrationTracker>();
				});
			return webHostBuilder;
		}

		public static IWebHostBuilder AddLogging(this IWebHostBuilder webHostBuilder)
		{
			var category = "";

			webHostBuilder.ConfigureLogging(
				(context, logging) =>
				{
					logging.ClearProviders();
					logging.AddConsole();
					logging.AddApplicationInsights();

					category = GetConfig(context, "GENERAL_CONTEXT", "DddLogger");
					var dotNetLogLevel = GetLogLevel(context, "LOGGING_LEVEL_DOTNET", MsLogLevel.Debug);
					var logLevel = GetLogLevel(context, "LOGGING_LEVEL", MsLogLevel.Debug);

					logging.AddFilter("", dotNetLogLevel);
					logging.AddFilter(category, logLevel);
					logging.AddFilter<ApplicationInsightsLoggerProvider>(category, logLevel);
				});

			webHostBuilder.ConfigureServices((context, services) =>
				services.AddSingleton<Logging.ILogger, Logger>(sp =>
					new Logger(
						sp.GetRequiredService<ISettings>(),
						sp.GetRequiredService<ILoggerFactory>().CreateLogger(category))));

			return webHostBuilder;
		}

		// Helpers

		private static string GetConfig(
			WebHostBuilderContext context,
			string cfgKey,
			string defaultValue = null)
		{
			var keysExists = context.Configuration.GetChildren().Any(x => x.Key == cfgKey);

			if (!keysExists)
				return defaultValue;

			return context.Configuration[cfgKey];
		}

		private static MsLogLevel GetLogLevel(
			WebHostBuilderContext context,
			string cfgKey,
			MsLogLevel defaultLevel = MsLogLevel.Debug)
		{
			var value = GetConfig(context, cfgKey, defaultLevel.ToString());

			var success = Enum.TryParse<LogLevel>(value, true, out var logLevel);

			if (!success)
				throw new DddException(
				$"Couldn't get log level for config key '{cfgKey}', " +
				$"invalid value: '{value}'.");

			return ToMsLogLevel(logLevel);
		}

		private static MsLogLevel ToMsLogLevel(LogLevel logLevel)
		{
			var success = Enum.TryParse<MsLogLevel>(logLevel.ToString(), true, out var msLogLevel);

			if (!success)
				throw new DddException(
					$"Couldn't convert log level '{logLevel}' to microsoft log level.");

			return msLogLevel;
		}
	}
}
