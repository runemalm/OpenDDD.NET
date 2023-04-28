using System;
using System.Collections.Generic;
using Microsoft.Extensions.Options;

namespace OpenDDD.Application.Settings.Http
{
	public class HttpCorsSettings : IHttpCorsSettings
	{
		public IEnumerable<string> AllowedOrigins { get; }

		public HttpCorsSettings() { }

		public HttpCorsSettings(IOptions<Options> options)
		{
			// Allowed origins
			var allowedOrigins =
				options.Value.HTTP_CORS_ALLOWED_ORIGINS?.Split(
					new string[] { "," }, StringSplitOptions.None) ?? new string[0];

			AllowedOrigins = allowedOrigins;
		}
	}
}
