using OpenDDD.Application;
using Bookstore.Domain.Model;
using Bookstore.Domain.Service;

namespace Bookstore.Application.Actions.RegisterCustomer
{
    public class RegisterCustomerAction : IAction<RegisterCustomerCommand, Customer>
    {
        private readonly ICustomerDomainService _customerDomainService;
        private readonly ICustomerRepository _customerRepository;

        public RegisterCustomerAction(
            ICustomerDomainService customerDomainService,
            ICustomerRepository customerRepository)
        {
            _customerDomainService = customerDomainService;
            _customerRepository = customerRepository;
        }

        public async Task<Customer> ExecuteAsync(RegisterCustomerCommand command, CancellationToken ct)
        {
            var customer = await _customerDomainService.RegisterAsync(command.Name, command.Email, ct);
            await _customerRepository.SaveAsync(customer, ct);
            return customer;
        }
    }
}
