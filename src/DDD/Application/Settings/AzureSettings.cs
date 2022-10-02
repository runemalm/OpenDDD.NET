namespace DDD.Application.Settings
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
