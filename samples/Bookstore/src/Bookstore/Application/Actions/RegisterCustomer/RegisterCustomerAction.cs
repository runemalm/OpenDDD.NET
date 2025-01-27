using Bookstore.Domain.Model;
using Bookstore.Domain.Services;
using OpenDDD.Application;

namespace Bookstore.Application.Actions.RegisterCustomer
{
    public class RegisterCustomerAction : IAction<RegisterCustomerCommand, Customer>
    {
        private readonly ICustomerDomainService _customerDomainService;

        public RegisterCustomerAction(ICustomerDomainService customerDomainService)
        {
            _customerDomainService = customerDomainService;
        }

        public async Task<Customer> ExecuteAsync(RegisterCustomerCommand command, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(command.Name))
                throw new ArgumentException("Customer name cannot be empty.", nameof(command.Name));

            if (string.IsNullOrWhiteSpace(command.Email))
                throw new ArgumentException("Customer email cannot be empty.", nameof(command.Email));

            // Delegate the registration logic to the domain service
            var customer = await _customerDomainService.RegisterAsync(command.Name, command.Email, ct);
            return customer;
        }
    }
}
