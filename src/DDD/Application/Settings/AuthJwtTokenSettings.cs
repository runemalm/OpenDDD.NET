using Microsoft.Extensions.Options;

namespace DDD.Application.Settings
{
	public class AuthJwtTokenSettings : IAuthJwtTokenSettings
	{
		public string PrivateKey { get; }
		public string Name { get; }
		public string Location { get; }
		public string Scheme { get; }

		public AuthJwtTokenSettings() { }

		public AuthJwtTokenSettings(IOptions<Options> options)
		{
			var privateKey = options.Value.AUTH_JWT_TOKEN_PRIVATE_KEY;
			var name = options.Value.AUTH_JWT_TOKEN_NAME;
			var location = options.Value.AUTH_JWT_TOKEN_LOCATION;
			var scheme = options.Value.AUTH_JWT_TOKEN_SCHEME;

			PrivateKey = privateKey;
			Name = name;
			Location = location;
			Scheme = scheme;
		}
	}
}
