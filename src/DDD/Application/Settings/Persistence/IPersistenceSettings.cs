namespace DDD.Application.Settings.Persistence
{
	public interface IPersistenceSettings
	{
		PersistenceProvider Provider { get; set; }
	}
}
