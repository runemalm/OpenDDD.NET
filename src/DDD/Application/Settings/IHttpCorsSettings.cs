using System.Collections.Generic;

namespace DDD.Application.Settings
{
	public interface IHttpCorsSettings
	{
		IEnumerable<string> AllowedOrigins { get; }
	}
}
