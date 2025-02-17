using OpenDDD.Application;
using Bookstore.Domain.Model;
using Bookstore.Domain.Service;

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
            var customer = await _customerDomainService.RegisterAsync(command.Name, command.Email, ct);
            return customer;
        }
    }
}
