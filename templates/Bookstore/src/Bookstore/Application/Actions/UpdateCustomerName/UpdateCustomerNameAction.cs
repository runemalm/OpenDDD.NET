using OpenDDD.Application;
using Bookstore.Domain.Model;

namespace Bookstore.Application.Actions.UpdateCustomerName
{
    public class UpdateCustomerNameAction : IAction<UpdateCustomerNameCommand, Customer>
    {
        private readonly ICustomerRepository _customerRepository;

        public UpdateCustomerNameAction(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
        }

        public async Task<Customer> ExecuteAsync(UpdateCustomerNameCommand command, CancellationToken ct)
        {
            if (command == null) throw new ArgumentNullException(nameof(command));

            var customer = await _customerRepository.FindByEmailAsync(command.Email, ct);

            if (customer == null)
            {
                throw new InvalidOperationException($"Customer with email {command.Email} not found.");
            }

            customer.ChangeName(command.FullName);
            await _customerRepository.SaveAsync(customer, ct);
            return customer;
        }
    }
}
