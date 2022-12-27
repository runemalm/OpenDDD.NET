using DDD.Application.Settings;
using DDD.Domain.Model.Auth;
using DDD.Domain.Services.Auth;
using DDD.Logging;

namespace DDD.Domain.Services
{
	public class DomainService : IDomainService
	{
		protected readonly ICredentials _credentials;
		protected readonly ISettings _settings;
		protected readonly ILogger _logger;
		protected readonly IAuthDomainService _authDomainService;

		public DomainService(
			ICredentials credentials,
			ISettings settings,
			ILogger logger,
			IAuthDomainService authDomainService)
		{
			_credentials = credentials;
			_settings = settings;
			_logger = logger;
			_authDomainService = authDomainService;
		}
	}
}
