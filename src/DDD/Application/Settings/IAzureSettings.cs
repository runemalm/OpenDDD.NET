namespace DDD.Application.Settings
{
	public interface IAzureSettings
	{
		public IServiceBusSettings ServiceBus { get; }
	}
}
