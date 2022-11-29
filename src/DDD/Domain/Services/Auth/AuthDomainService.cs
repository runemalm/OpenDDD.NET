using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DDD.Application;
using DDD.Application.Settings;
using DDD.Domain.Model.Auth;
using DDD.Domain.Model.Auth.Exceptions;
using DDD.Infrastructure.Ports.Auth;
using DDD.Logging;

namespace DDD.Domain.Services.Auth
{
	public class AuthDomainService : IAuthDomainService
	{
		private readonly ICredentials _credentials;
		private readonly ISettings _settings;
		private readonly ILogger _logger;
		private readonly IIamPort _iamAdapter;

		public AuthDomainService(
			ICredentials credentials,
			ISettings settings,
			ILogger logger,
			IIamPort iamAdapter)
		{
			_credentials = credentials;
			_settings = settings;
			_logger = logger;
			_iamAdapter = iamAdapter;
		}
		
		// Authenticated
		
		public Task AssureAuthenticatedAsync(ActionId actionId, CancellationToken ct)
		{
			if (!_settings.Auth.Enabled) return Task.CompletedTask;
			
			if (_credentials.JwtToken == null)
				throw new MissingJwtTokenException();
			
			if (_credentials.JwtToken.UserId == null)
				throw new NotAuthenticatedException();
			
			return Task.CompletedTask;
		}
		
		// Role names in token

		public async Task AssureRolesInTokenAsync(
			IEnumerable<IEnumerable<string>> roles,
			ActionId actionId,
			CancellationToken ct)
		{
			if (!_settings.Auth.Enabled) return;
			
			await AssureAuthenticatedAsync(actionId, ct);

			_credentials.JwtToken.Validate(_settings.Auth.JwtToken.PrivateKey, roles);
		}
		
		// RBAC

		public async Task AssurePermissionsInWorldAsync(
			string domain, 
			IEnumerable<string> permissions, 
			ActionId actionId, 
			CancellationToken ct)
		{
			if (!_settings.Auth.Enabled) return;

			await AssureAuthenticatedAsync(actionId, ct);

			var hasPermissions = await _iamAdapter.HasPermissionsInWorldAsync(domain, permissions, actionId, ct);

			if (!hasPermissions)
				throw new ForbiddenException();
		}

		public async Task AssurePermissionsInRealmAsync(
			string externalRealmId, 
			string domain, 
			IEnumerable<string> permissions, 
			ActionId actionId, 
			CancellationToken ct)
		{
			if (!_settings.Auth.Enabled) return;
			
			await AssureAuthenticatedAsync(actionId, ct);
			
			var hasPermissions = await _iamAdapter.HasPermissionsInRealmAsync(externalRealmId, domain, permissions, actionId, ct);

			if (!hasPermissions)
				throw new ForbiddenException();
		}


		public async Task AssurePermissionsInResourceGroupAsync(
			string externalRealmId,
			string resourceGroupId,
			string domain, 
			IEnumerable<string> permissions, 
			ActionId actionId, 
			CancellationToken ct)
		{
			if (!_settings.Auth.Enabled) return;
			
			await AssureAuthenticatedAsync(actionId, ct);
			
			var hasPermissions = await _iamAdapter.HasPermissionsInResourceGroupAsync(externalRealmId, resourceGroupId, domain, permissions, actionId, ct);

			if (!hasPermissions)
				throw new ForbiddenException();
		}

		public async Task AssurePermissionsInResourceAsync(
			string externalRealmId,
			string resourceId, 
			string domain, 
			IEnumerable<string> permissions, 
			ActionId actionId, 
			CancellationToken ct)
		{
			if (!_settings.Auth.Enabled) return;
			
			await AssureAuthenticatedAsync(actionId, ct);
			
			var hasPermissions = await _iamAdapter.HasPermissionsInResourceAsync(externalRealmId, resourceId, domain, permissions, actionId, ct);

			if (!hasPermissions)
				throw new ForbiddenException();
		}
	}
}
