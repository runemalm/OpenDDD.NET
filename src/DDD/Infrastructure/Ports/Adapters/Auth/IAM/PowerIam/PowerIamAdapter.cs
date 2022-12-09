using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using PowerIAM.Api;
using PowerIAM.Client;
using DDD.Application;
using DDD.Application.Settings;
using DDD.Domain.Model.Auth;
using DDD.Infrastructure.Ports.Auth;
using DDD.Logging;

namespace DDD.Infrastructure.Ports.Adapters.Auth.IAM.PowerIam
{
	public class PowerIamAdapter : IIamPort
	{
		private readonly ISettings _settings;
		private readonly ILogger _logger;
		private readonly ICredentials _credentials;
		private V102Api _api { get; set; }

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
			
			_api = new V102Api(config);
		}

		public Task<bool> HasPermissionsInWorldAsync(
			string domain, 
			IEnumerable<string> permissions, 
			ActionId actionId, 
			CancellationToken ct)
			=> _api.AssurePermissionsInWorldAsync(
				domain, 
				permissions.ToList(), 
				0, 
				ct);

		public Task<bool> HasPermissionsInRealmAsync(
			string realmId,
			string externalRealmId,
			string domain,
			IEnumerable<string> permissions,
			ActionId actionId,
			CancellationToken ct)
			=> _api.AssurePermissionsInRealmAsync(
				realmId, 
				externalRealmId, 
				domain, 
				permissions.ToList(), 
				0, 
				ct);

		public Task<bool> HasPermissionsInResourceGroupAsync(
			string resourceGroupId, 
			string domain, 
			IEnumerable<string> permissions,
			ActionId actionId, CancellationToken ct)
		{
			throw new System.NotImplementedException();
		}

		public Task<bool> HasPermissionsInResourceAsync(
			string resourceId, 
			string domain, 
			IEnumerable<string> permissions, 
			ActionId actionId,
			CancellationToken ct)
		{
			throw new System.NotImplementedException();
		}
	}
}
