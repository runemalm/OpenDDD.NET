namespace DDD.Domain.Auth
{
	public class Credentials : ICredentials
	{
		public JwtToken JwtToken { get; set; }

		public Credentials()
		{

		}
	}
}
