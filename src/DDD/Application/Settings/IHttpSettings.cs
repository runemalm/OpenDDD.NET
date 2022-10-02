namespace DDD.Application.Settings
{
	public interface IHttpSettings
	{
		IHttpCorsSettings Cors { get; }
		IHttpDocsSettings Docs { get; }
	}
}
