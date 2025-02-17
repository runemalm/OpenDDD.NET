using OpenDDD.Application;

namespace Bookstore.Application.Actions.SendWelcomeEmail
{
    public class SendWelcomeEmailCommand : ICommand
    {
        public string RecipientEmail { get; set; }
        public string RecipientName { get; set; }

        public SendWelcomeEmailCommand(string recipientEmail, string recipientName)
        {
            RecipientEmail = recipientEmail;
            RecipientName = recipientName;
        }
    }
}
