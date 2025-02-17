using OpenDDD.Application;

namespace Bookstore.Application.Actions.GetCustomer
{
    public class GetCustomerCommand : ICommand
    {
        public Guid CustomerId { get; }
        
        public GetCustomerCommand() { }

        public GetCustomerCommand(Guid customerId)
        {
            CustomerId = customerId;
        }
    }
}
