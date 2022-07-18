namespace DDD.Application.Settings
{
	public interface IPersistenceSettings
	{
		PersistenceProvider Provider { get; set; }
	}
}
