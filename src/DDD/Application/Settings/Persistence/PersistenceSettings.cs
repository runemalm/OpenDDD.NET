using Microsoft.Extensions.Options;

namespace DDD.Application.Settings.Persistence
{
	public class PersistenceSettings : IPersistenceSettings
	{
		public PersistenceProvider Provider { get; set; }
		
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
		}
	}
}
