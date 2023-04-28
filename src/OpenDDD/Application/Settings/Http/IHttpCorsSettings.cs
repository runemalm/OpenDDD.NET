using System.Collections.Generic;

namespace OpenDDD.Application.Settings.Http
{
	public interface IHttpCorsSettings
	{
		IEnumerable<string> AllowedOrigins { get; }
	}
}
