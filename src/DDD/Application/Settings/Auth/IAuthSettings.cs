namespace DDD.Application.Settings.Auth
{
	public interface IAuthSettings
	{
		bool Enabled { get; }
		public IAuthRbacSettings Rbac { get; set; }
		IAuthJwtTokenSettings JwtToken { get; }
	}
}
