using Microsoft.Extensions.DependencyInjection;
using OpenDDD.Application.Settings;
using Infrastructure.Ports.Adapters.Common.Translation.Converters;
using OpenDddConversionSettings = OpenDDD.Infrastructure.Ports.Adapters.Common.Translation.Converters.ConversionSettings;

namespace Main.Extensions
{
	public static class ServiceCollectionExtensions
	{
		// Public API

		public static IServiceCollection AddConversion(this IServiceCollection services, ISettings settings)
		{
			services.AddTransient<OpenDddConversionSettings, ConversionSettings>();
			return services;
		}
	}
}
