using OpenDDD.Application;

namespace Bookstore.Application.Actions.RegisterCustomer
{
    public class RegisterCustomerCommand : ICommand
    {
        public string Name { get; set; }
        public string Email { get; set; }

        public RegisterCustomerCommand() { }

        public RegisterCustomerCommand(string name, string email)
        {
            Name = name;
            Email = email;
        }
    }
}
