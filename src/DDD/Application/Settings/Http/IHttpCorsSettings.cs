using System.Collections.Generic;

namespace DDD.Application.Settings.Http
{
	public interface IHttpCorsSettings
	{
		IEnumerable<string> AllowedOrigins { get; }
	}
}
