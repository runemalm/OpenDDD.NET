namespace OpenDDD.Application.Settings.Auth
{
	public interface IAuthRbacSettings
	{
		RbacProvider Provider { get; }
		string ExternalRealmId { get; }
	}
}
