namespace DDD.Application.Settings
{
	public interface IAuthSettings
	{
		bool Enabled { get; }
		IAuthJwtTokenSettings JwtToken { get; }
	}
}
