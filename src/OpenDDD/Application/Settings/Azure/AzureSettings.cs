using OpenDDD.Application.Settings.ServiceBus;

namespace OpenDDD.Application.Settings.Azure
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
