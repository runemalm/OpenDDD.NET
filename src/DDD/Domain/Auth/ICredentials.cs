namespace DDD.Domain.Auth
{
	public interface ICredentials
	{
		JwtToken JwtToken { get; set; }
	}
}
