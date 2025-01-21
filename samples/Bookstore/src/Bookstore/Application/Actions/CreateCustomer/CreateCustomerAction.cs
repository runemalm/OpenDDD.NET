using Bookstore.Domain.Model;
using OpenDDD.Application;

namespace Bookstore.Application.Actions.CreateCustomer
{
    public class CreateCustomerAction : IAction<CreateCustomerCommand, Customer>
    {
        private readonly ICustomerRepository _customerRepository;

        public CreateCustomerAction(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<Customer> ExecuteAsync(CreateCustomerCommand command, CancellationToken ct)
        {
            throw new NotImplementedException();
        }
    }
}
