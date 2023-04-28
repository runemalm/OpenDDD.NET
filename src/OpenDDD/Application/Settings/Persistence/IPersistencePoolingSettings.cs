namespace OpenDDD.Application.Settings.Persistence
{
	public interface IPersistencePoolingSettings
	{
		bool Enabled { get; }
		int MinSize { get; }
		int MaxSize { get; }
	}
}
