using Microsoft.Extensions.Options;

namespace DDD.Application.Settings.Http
{
	public class HttpSettings : IHttpSettings
	{
		public IHttpCorsSettings Cors { get; set; }
		public IHttpDocsSettings Docs { get; set; }

		public HttpSettings() { }

		public HttpSettings(IOptions<Options> options)
		{
			Cors = new HttpCorsSettings(options);
			Docs = new HttpDocsSettings(options);
		}
	}
}
