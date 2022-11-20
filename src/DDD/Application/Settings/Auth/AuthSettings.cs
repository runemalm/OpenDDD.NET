using DDD.NETCore.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DDD.Application.Settings.Auth
{
	public class AuthSettings : IAuthSettings
	{
		public bool Enabled { get; }
		public IAuthJwtTokenSettings JwtToken { get; }
		public AuthSettings() { }

		public AuthSettings(IOptions<Options> options)
		{
			var enabled = options.Value.AUTH_ENABLED.IsTrue();

			// JWT Token
			var jwtToken = new AuthJwtTokenSettings(options);
			
			// Azure OIDC
			Enabled = enabled;
			JwtToken = jwtToken;
			
			if (Enabled && JwtToken.PrivateKey.IsNullOrEmpty())
				throw new SettingsException(
					$"Settings auth enabled with no or empty private key is not allowed.");
		}
	}
}
