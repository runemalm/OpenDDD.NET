using OpenDDD.Application;

namespace Bookstore.Application.Actions.UpdateCustomerName
{
    public class UpdateCustomerNameCommand : ICommand
    {
        public string Email { get; }
        public string FullName { get; }

        public UpdateCustomerNameCommand(string email, string fullName)
        {
            Email = email;
            FullName = fullName;
        }
    }
}
