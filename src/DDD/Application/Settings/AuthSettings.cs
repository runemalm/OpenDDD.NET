using DDD.DotNet.Extensions;
using Microsoft.Extensions.Options;

namespace DDD.Application.Settings
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
		}
	}
}
