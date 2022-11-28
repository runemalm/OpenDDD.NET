using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DDD.Application;
using DDD.Application.Settings;
using DDD.Domain.Model.Auth;
using DDD.Domain.Model.Auth.Exceptions;
using DDD.Logging;

namespace DDD.Domain.Services.Auth
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

		public Task AssureAuthenticatedAsync(ActionId actionId, CancellationToken ct)
		{
			if (!_settings.Auth.Enabled) return Task.CompletedTask;
			
			throw new System.NotImplementedException();
		}

		public Task AssurePermissionsInWorldAsync(string domainName, IEnumerable<string> permissions, ActionId actionId, CancellationToken ct)
		{
			if (!_settings.Auth.Enabled) return Task.CompletedTask;
			
			throw new System.NotImplementedException();
		}

		public Task AssurePermissionsInRealmAsync(string realmId, string domainName, IEnumerable<string> permissions, ActionId actionId, CancellationToken ct)
		{
			if (!_settings.Auth.Enabled) return Task.CompletedTask;
			
			throw new System.NotImplementedException();
		}


		public Task AssurePermissionsInResourceGroupAsync(string resourceGroupId, string domainName, IEnumerable<string> permissions, ActionId actionId, CancellationToken ct)
		{
			if (!_settings.Auth.Enabled) return Task.CompletedTask;
			
			throw new System.NotImplementedException();
		}

		public Task AssurePermissionsInResourceAsync(string resourceId, string domainName, IEnumerable<string> permissions, ActionId actionId, CancellationToken ct)
		{
			if (!_settings.Auth.Enabled) return Task.CompletedTask;
			
			throw new System.NotImplementedException();
		}
		
		// Role-names-in-token mode

		public async Task AssureRolesInTokenAsync(
			IEnumerable<IEnumerable<string>> roles,
			ActionId actionId,
			CancellationToken ct)
		{
			if (!_settings.Auth.Enabled) return;
			
			if (_credentials.JwtToken == null)
				throw new MissingCredentialsException("No JWT token available.");
		
			_credentials.JwtToken.Validate(_settings.Auth.JwtToken.PrivateKey, roles);
		}
	}
}
