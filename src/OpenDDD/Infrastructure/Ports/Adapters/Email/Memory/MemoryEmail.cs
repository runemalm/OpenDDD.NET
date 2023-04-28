namespace OpenDDD.Infrastructure.Ports.Adapters.Email.Memory
{
	public class MemoryEmail : IEmail
	{
		public string ToEmail { get; set; }
		public string Message { get; set; }
			
		public static MemoryEmail Create(string toEmail, string message)
		{
			var memoryEmail = new MemoryEmail { ToEmail = toEmail, Message = message };
			return memoryEmail;
		}
	}
}
