using OpenDDD.Application.Settings.ServiceBus;

namespace OpenDDD.Application.Settings.Azure
{
	public interface IAzureSettings
	{
		public IServiceBusSettings ServiceBus { get; }
	}
}
