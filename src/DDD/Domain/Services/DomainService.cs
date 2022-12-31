using DDD.Application.Settings;
using DDD.Domain.Model.Auth;
using DDD.Logging;

namespace DDD.Domain.Services
{
	public class DomainService : IDomainService
	{
		protected readonly ICredentials _credentials;
		protected readonly ISettings _settings;
		protected readonly ILogger _logger;

		public DomainService(
			ICredentials credentials,
			ISettings settings,
			ILogger logger)
		{
			_credentials = credentials;
			_settings = settings;
			_logger = logger;
		}
	}
}
