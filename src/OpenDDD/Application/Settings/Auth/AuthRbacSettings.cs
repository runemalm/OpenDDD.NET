using Microsoft.Extensions.Options;

namespace OpenDDD.Application.Settings.Auth
{
	public class AuthRbacSettings : IAuthRbacSettings
	{
		public RbacProvider Provider { get; set; }
		public string ExternalRealmId { get; }

		public AuthRbacSettings(IOptions<Options> options)
		{
			// RBAC provider
			var rbacProvider = RbacProvider.None;
			var providerString = options.Value.AUTH_RBAC_PROVIDER;
			if (providerString != null)
				if (providerString.ToLower() == "negative")
					rbacProvider = RbacProvider.Negative;
				else if (providerString.ToLower() == "positive")
					rbacProvider = RbacProvider.Positive;
				else if (providerString.ToLower() == "poweriam")
					rbacProvider = RbacProvider.PowerIAM;
				else
					throw SettingsException.Invalid($"Unsupported RBAC setting: '{providerString}'.");
			Provider = rbacProvider;
			
			// External realm ID
			ExternalRealmId = options.Value.AUTH_RBAC_EXTERNAL_REALM_ID;
		}
	}
}
