using Microsoft.Extensions.Options;

namespace DDD.Application.Settings
{
	public class PostgresSettings : IPostgresSettings
	{
		public string ConnString { get; }
		
		public PostgresSettings() { }

		public PostgresSettings(IOptions<Options> options)
		{
			var connString = options.Value.POSTGRES_CONN_STR;
			
			ConnString = connString;
		}
	}
}
