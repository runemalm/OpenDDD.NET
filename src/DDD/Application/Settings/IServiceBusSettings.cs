namespace DDD.Application.Settings
{
	public interface IServiceBusSettings
	{
		string ConnString { get; }
		string SubName { get; }
	}
}
