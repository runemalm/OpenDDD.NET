using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PowerIAM.Api;
using PowerIAM.Client;
using OpenDDD.Application;
using OpenDDD.Application.Settings;
using OpenDDD.Domain.Model.Auth;
using OpenDDD.Infrastructure.Ports.Auth;
using OpenDDD.Logging;

namespace OpenDDD.Infrastructure.Ports.Adapters.Auth.IAM.PowerIam
{
	public class PowerIamAdapter : IIamPort
	{
		private readonly ISettings _settings;
		private readonly ILogger _logger;
		private readonly ICredentials _credentials;
		private AccessApi _accessApi { get; set; }

		public PowerIamAdapter(ISettings settings, ILogger logger, ICredentials credentials)
		{
			_settings = settings;
			_logger = logger;
			_credentials = credentials;

			CreateClient();
		}

		private void CreateClient()
		{
			Configuration config = new Configuration();
			
			config.BasePath = _settings.PowerIam.Url;
			
			if (_credentials.JwtToken != null)
				config.ApiKey.Add("Authorization", $"Bearer {_credentials.JwtToken.RawString}");
			
			_accessApi = new AccessApi(config);
		}

		public Task<bool> HasPermissionsInWorldAsync(
			IEnumerable<(string, string)> permissions, 
			ActionId actionId, 
			CancellationToken ct)
			=> _accessApi.AssurePermissionsAsync(
				null,
				null,
				permissions.Select(t => $"{t.Item1}:{t.Item2}").ToList(),
				0,
				ct);

		public Task<bool> HasPermissionsInRealmAsync(
			string realmId,
			string externalRealmId,
			IEnumerable<(string, string)> permissions,
			ActionId actionId,
			CancellationToken ct)
			=> _accessApi.AssurePermissionsAsync(
				realmId,
				externalRealmId,
				permissions.Select(t => $"{t.Item1}:{t.Item2}").ToList(),
				0,
				ct);

		public Task<bool> HasPermissionsInRealmAsync(
			string realmId,
			string externalRealmId,
			IEnumerable<(string, string)> permissions,
			string actorId,
			ActionId actionId,
			CancellationToken ct)
			=> throw new NotImplementedException("Need to implement endpoint in api that allows passing the new 'actorId' argument.");

		public Task<bool> HasPermissionsInResourceGroupAsync(
			string resourceGroupId, 
			string domain, 
			IEnumerable<(string, string)> permissions,
			ActionId actionId, CancellationToken ct)
		{
			throw new System.NotImplementedException();
		}

		public Task<bool> HasPermissionsInResourceAsync(
			string resourceId, 
			string domain, 
			IEnumerable<(string, string)> permissions, 
			ActionId actionId,
			CancellationToken ct)
		{
			throw new System.NotImplementedException();
		}
	}
}
