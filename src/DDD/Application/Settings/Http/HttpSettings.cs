using System;
using System.Collections.Generic;
using Microsoft.Extensions.Options;

namespace DDD.Application.Settings.Http
{
	public class HttpSettings : IHttpSettings
	{
		public IEnumerable<string> Urls { get; }
		public IHttpCorsSettings Cors { get; set; }
		public IHttpDocsSettings Docs { get; set; }

		public HttpSettings() { }

		public HttpSettings(IOptions<Options> options)
		{
			Urls =
				options.Value.HTTP_URLS?.Split(
					new string[] { "," }, StringSplitOptions.None) ?? new string[0];
			Cors = new HttpCorsSettings(options);
			Docs = new HttpDocsSettings(options);
		}
	}
}
