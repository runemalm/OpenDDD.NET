using OpenDDD.Application;

namespace Bookstore.Application.Actions.CreateCustomer
{
    public class CreateCustomerCommand : ICommand
    {
        public string Name { get; set; }
        public string Email { get; set; }

        public CreateCustomerCommand() { }

        public CreateCustomerCommand(string name, string email)
        {
            Name = name;
            Email = email;
        }
    }
}
