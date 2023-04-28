using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OpenDDD.Application;
using OpenDDD.Application.Settings;
using OpenDDD.Domain.Model.Auth;
using OpenDDD.Domain.Model.Auth.Exceptions;
using OpenDDD.Infrastructure.Ports.Auth;
using OpenDDD.Logging;

namespace OpenDDD.Domain.Services.Auth
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
				throw AuthorizeException.MissingCredentials("No JwtToken was provided.");
			
			if (_credentials.JwtToken.UserId == null)
				throw AuthorizeException.NotAuthenticated();
			
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
			IEnumerable<(string, string)> permissions,
			ActionId actionId, 
			CancellationToken ct)
		{
			if (!_settings.Auth.Enabled) return;

			await AssureAuthenticatedAsync(actionId, ct);

			var hasPermissions = 
				await _iamAdapter.HasPermissionsInWorldAsync(
					permissions, 
					actionId, 
					ct);

			if (!hasPermissions)
				throw AuthorizeException.Forbidden();
		}

		public async Task AssurePermissionsInRealmAsync(
			string? realmId,
			string? externalRealmId, 
			IEnumerable<(string, string)> permissions, 
			ActionId actionId, 
			CancellationToken ct)
		{
			if (!_settings.Auth.Enabled) return;
			
			await AssureAuthenticatedAsync(actionId, ct);
			
			var hasPermissions = 
				await _iamAdapter.HasPermissionsInRealmAsync(
					realmId,
					externalRealmId, 
					permissions, 
					actionId, 
					ct);

			if (!hasPermissions)
				throw AuthorizeException.Forbidden();
		}


		public async Task AssurePermissionsInResourceGroupAsync(
			string resourceGroupId,
			string externalResourceGroupId,
			IEnumerable<(string, string)> permissions, 
			ActionId actionId, 
			CancellationToken ct)
		{
			if (!_settings.Auth.Enabled) return;
			
			await AssureAuthenticatedAsync(actionId, ct);
			
			var hasPermissions = 
				await _iamAdapter.HasPermissionsInResourceGroupAsync(
					resourceGroupId, 
					externalResourceGroupId,
					permissions, 
					actionId, 
					ct);

			if (!hasPermissions)
				throw AuthorizeException.Forbidden();
		}

		public async Task AssurePermissionsInResourceAsync(
			string resourceId,
			string externalResourceId,
			IEnumerable<(string, string)> permissions,
			ActionId actionId, 
			CancellationToken ct)
		{
			if (!_settings.Auth.Enabled) return;
			
			await AssureAuthenticatedAsync(actionId, ct);
			
			var hasPermissions = 
				await _iamAdapter.HasPermissionsInResourceAsync(
					resourceId, 
					externalResourceId, 
					permissions, 
					actionId, 
					ct);

			if (!hasPermissions)
				throw AuthorizeException.Forbidden();
		}

		public Task<bool> HasPermissionsInWorldAsync(string domain, IEnumerable<string> permissions, ActionId actionId, CancellationToken ct)
		{
			throw new System.NotImplementedException();
		}

		public Task<bool> HasPermissionsInRealmAsync(string realmId, string externalRealmId, string domain, IEnumerable<string> permissions,
			ActionId actionId, CancellationToken ct)
		{
			throw new System.NotImplementedException();
		}
	}
}
