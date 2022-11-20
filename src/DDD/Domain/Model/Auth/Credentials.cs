namespace DDD.Domain.Model.Auth
{
	public class Credentials : ICredentials
	{
		public JwtToken JwtToken { get; set; }

		public Credentials()
		{
			
		}
	}
}
