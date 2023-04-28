namespace OpenDDD.Application.Settings.Auth
{
	public interface IAuthJwtTokenSettings
	{
		string PrivateKey { get; }
		string Name { get; }
		string Location { get; }
		string Scheme { get; }
	}
}
