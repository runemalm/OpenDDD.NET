namespace DDD.Application.Settings.Http
{
	public interface IHttpSettings
	{
		IHttpCorsSettings Cors { get; }
		IHttpDocsSettings Docs { get; }
	}
}
