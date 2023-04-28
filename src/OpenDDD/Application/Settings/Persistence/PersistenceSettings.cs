using Microsoft.Extensions.Options;

namespace OpenDDD.Application.Settings.Persistence
{
	public class PersistenceSettings : IPersistenceSettings
	{
		public PersistenceProvider Provider { get; set; }
		public IPersistencePoolingSettings Pooling { get; set; }

		public PersistenceSettings() { }

		public PersistenceSettings(IOptions<Options> options)
		{
			var provider = PersistenceProvider.None;
			var providerString = options.Value.PERSISTENCE_PROVIDER;
			if (providerString != null)
				if (providerString.ToLower() == "memory")
					provider = PersistenceProvider.Memory;
				else if (providerString.ToLower() == "postgres")
					provider = PersistenceProvider.Postgres;

			Provider = provider;
			Pooling = new PersistencePoolingSettings(options);
		}
	}
}
