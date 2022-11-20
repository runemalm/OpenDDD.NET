using DDD.Application.Settings.ServiceBus;

namespace DDD.Application.Settings.Azure
{
	public class AzureSettings : IAzureSettings
	{
		public IServiceBusSettings ServiceBus { get; }

		public AzureSettings() { }

		public AzureSettings(IServiceBusSettings serviceBusSettings)
		{
			ServiceBus = serviceBusSettings;
		}
	}
}
