using DDD.Application.Settings.ServiceBus;

namespace DDD.Application.Settings.Azure
{
	public interface IAzureSettings
	{
		public IServiceBusSettings ServiceBus { get; }
	}
}
