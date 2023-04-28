using System.Collections.Generic;

namespace OpenDDD.Application.Settings.Http
{
	public interface IHttpSettings
	{
		IEnumerable<string> Urls { get; }
		IHttpCorsSettings Cors { get; }
		IHttpDocsSettings Docs { get; }
	}
}
