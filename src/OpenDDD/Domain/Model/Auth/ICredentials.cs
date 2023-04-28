namespace OpenDDD.Domain.Model.Auth
{
	public interface ICredentials
	{
		JwtToken JwtToken { get; set; }
	}
}
