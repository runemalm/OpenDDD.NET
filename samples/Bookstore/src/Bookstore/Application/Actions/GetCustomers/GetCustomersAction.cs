using OpenDDD.Application;
using Bookstore.Domain.Model;

namespace Bookstore.Application.Actions.GetCustomers
{
    public class GetCustomersAction : IAction<GetCustomersCommand, IEnumerable<Customer>>
    {
        private readonly ICustomerRepository _customerRepository;

        public GetCustomersAction(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<IEnumerable<Customer>> ExecuteAsync(GetCustomersCommand command, CancellationToken ct)
        {
            var customers = await _customerRepository.FindAllAsync(ct);
            return customers;
        }
    }
}
