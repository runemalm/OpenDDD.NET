using Application.Settings;
using Application.Settings.Frontend;
using MsLogLevel = Microsoft.Extensions.Logging.LogLevel;
using DddSettings = DDD.Application.Settings.Settings;

namespace Main.Extensions
{
	public static class WebHostBuilderExtensions
	{
		// Public API

		public static IWebHostBuilder AddCustomSettings(this IWebHostBuilder webHostBuilder)
		{
			webHostBuilder =
				webHostBuilder.ConfigureServices((context, services) =>
				{
					services.Configure<CustomOptions>(context.Configuration);

					services.AddTransient<IFrontendSettings, FrontendSettings>();
					services.AddTransient<ICustomSettings, CustomSettings>();
					
				});
			return webHostBuilder;
		}
	}
}
