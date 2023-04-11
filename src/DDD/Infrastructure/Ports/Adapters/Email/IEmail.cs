namespace DDD.Infrastructure.Ports.Adapters.Email
{
	public interface IEmail
	{
		string ToEmail { get; set; }
		string Message { get; set; }
	}
}
