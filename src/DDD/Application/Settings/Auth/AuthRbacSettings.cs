using Microsoft.Extensions.Options;

namespace DDD.Application.Settings.Auth
{
	public class AuthRbacSettings : IAuthRbacSettings
	{
		public RbacProvider Provider { get; set; }
		public string ExternalRealmId { get; }

		public AuthRbacSettings() { }

		public AuthRbacSettings(IOptions<Options> options)
		{
			// RBAC provider
			var rbacProvider = RbacProvider.None;
			var providerString = options.Value.AUTH_RBAC_PROVIDER;
			if (providerString != null)
				if (providerString.ToLower() == "negative")
					rbacProvider = RbacProvider.Negative;
				else if (providerString.ToLower() == "poweriam")
					rbacProvider = RbacProvider.PowerIAM;
			Provider = rbacProvider;
			
			// External realm ID
			ExternalRealmId = options.Value.AUTH_RBAC_EXTERNAL_REALM_ID;
		}
	}
}
