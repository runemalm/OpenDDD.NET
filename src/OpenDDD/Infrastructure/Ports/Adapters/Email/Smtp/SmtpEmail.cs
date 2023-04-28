namespace OpenDDD.Infrastructure.Ports.Adapters.Email.Smtp
{
	public class SmtpEmail : IEmail
	{
		public string ToEmail { get; set; }
		public string Message { get; set; }
			
		public static SmtpEmail Create(string toEmail, string message)
		{
			var smtpEmail = new SmtpEmail { ToEmail = toEmail, Message = message };
			return smtpEmail;
		}
	}
}
