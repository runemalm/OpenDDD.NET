using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DDD.Domain.Auth.Exceptions;
using DDD.Logging;
using DDD.Application.Settings;

namespace DDD.Domain.Auth
{
	public class AuthDomainService : IAuthDomainService
	{
		private readonly ICredentials _credentials;
		private readonly ISettings _settings;
		private readonly ILogger _logger;

		public AuthDomainService(
			ICredentials credentials,
			ISettings settings,
			ILogger logger)
		{
			_credentials = credentials;
			_settings = settings;
			_logger = logger;
		}

		public Task AuthorizeRolesAsync(
			IEnumerable<IEnumerable<string>> roles,
			CancellationToken ct)
		{
			if (_settings.Auth.Enabled)
			{
				CheckRolesInToken(roles);
			}
			return Task.CompletedTask;
		}
		
		public void CheckRolesInToken(IEnumerable<IEnumerable<string>> roles)
		{
			if (_credentials.JwtToken == null)
				throw new MissingCredentialsException("No JWT token available.");

			_credentials.JwtToken.Validate(_settings.Auth.JwtToken.PrivateKey, roles);
		}
	}
}
