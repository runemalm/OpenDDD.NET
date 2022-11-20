namespace DDD.Application.Settings.ServiceBus
{
	public interface IServiceBusSettings
	{
		string ConnString { get; }
		string SubName { get; }
	}
}
