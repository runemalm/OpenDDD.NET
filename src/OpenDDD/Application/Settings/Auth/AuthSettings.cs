using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using OpenDDD.NET.Extensions;

namespace OpenDDD.Application.Settings.Auth
{
	public class AuthSettings : IAuthSettings
	{
		public bool Enabled { get; }
		public IAuthRbacSettings Rbac { get; set; }
		public IAuthJwtTokenSettings JwtToken { get; }
		
		public AuthSettings() { }

		public AuthSettings(IOptions<Options> options)
		{
			// Enabled
			Enabled = options.Value.AUTH_ENABLED.IsTrue();

			Rbac = new AuthRbacSettings(options);

			// JWT Token
			JwtToken = new AuthJwtTokenSettings(options);

			// Validate
			if (Enabled && JwtToken.PrivateKey.IsNullOrEmpty())
				throw SettingsException.AuthEnabledButNoPrivateKey();
		}
	}
}
