namespace OpenDDD.Application.Settings.Persistence
{
	public interface IPersistenceSettings
	{
		PersistenceProvider Provider { get; set; }
		IPersistencePoolingSettings Pooling { get; set; }
	}
}
