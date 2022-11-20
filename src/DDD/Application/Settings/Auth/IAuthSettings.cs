namespace DDD.Application.Settings.Auth
{
	public interface IAuthSettings
	{
		bool Enabled { get; }
		IAuthJwtTokenSettings JwtToken { get; }
	}
}
