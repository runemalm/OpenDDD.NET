using Bookstore.Domain.Model;
using OpenDDD.Application;

namespace Bookstore.Application.Actions.GetCustomer
{
    public class GetCustomerAction : IAction<GetCustomerCommand, Customer>
    {
        private readonly ICustomerRepository _customerRepository;

        public GetCustomerAction(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<Customer> ExecuteAsync(GetCustomerCommand command, CancellationToken ct)
        {
            var customer = await _customerRepository.GetAsync(command.CustomerId, ct);
            return customer;
        }
    }
}
