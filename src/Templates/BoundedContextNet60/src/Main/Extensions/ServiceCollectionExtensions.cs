using DDD.Application.Settings;
using Infrastructure.Ports.Adapters.Common.Translation.Converters;
using DddSerializerSettings = DDD.Infrastructure.Ports.Adapters.Common.Translation.Converters.SerializerSettings;

namespace Main.Extensions
{
	public static class ServiceCollectionExtensions
	{
		// Public API

		// public static IServiceCollection AddIdpAdapters(this IServiceCollection services)
		// {
		// 	services.AddTransient<IFacebookOidcPort, FacebookOidcAdapter>();
		// 	services.AddTransient<ISimpleOidcPort, SimpleOidcAdapter>();
		// 	return services;
		// }

		public static IServiceCollection AddSerialization(this IServiceCollection services, ISettings settings)
		{
			services.AddTransient<DddSerializerSettings, SerializerSettings>();
			return services;
		}
	}
}
