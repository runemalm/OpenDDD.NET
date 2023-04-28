using Microsoft.Extensions.DependencyInjection;
using OpenDDD.Application.Settings;
using Infrastructure.Ports.Adapters.Common.Translation.Converters;
using OpenDddSerializerSettings = OpenDDD.Infrastructure.Ports.Adapters.Common.Translation.Converters.SerializerSettings;

namespace Main.Extensions
{
	public static class ServiceCollectionExtensions
	{
		// Public API

		public static IServiceCollection AddSerialization(this IServiceCollection services, ISettings settings)
		{
			services.AddTransient<OpenDddSerializerSettings, SerializerSettings>();
			return services;
		}
	}
}
