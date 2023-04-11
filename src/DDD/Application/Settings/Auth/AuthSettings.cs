using DDD.Domain.Model.Error;
using DDD.NETCore.Extensions;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace DDD.Application.Settings.Auth
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
				throw new SettingsException(DomainError.AuthEnabledButNoPrivateKey());
		}
	}
}
