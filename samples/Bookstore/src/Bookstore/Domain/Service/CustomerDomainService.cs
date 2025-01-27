using Bookstore.Domain.Model;

namespace Bookstore.Domain.Service
{
    public class CustomerDomainService : ICustomerDomainService
    {
        private readonly ICustomerRepository _customerRepository;

        public CustomerDomainService(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
        }

        public async Task<Customer> RegisterAsync(string name, string email, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Customer name cannot be empty.", nameof(name));

            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Customer email cannot be empty.", nameof(email));

            // Check if a customer with the same email already exists
            var existingCustomer = await _customerRepository.FindByEmailAsync(email, ct);

            if (existingCustomer != null)
                throw new InvalidOperationException($"A customer with the email '{email}' already exists.");

            // Create a new customer
            var newCustomer = new Customer(Guid.NewGuid(), name, email);

            // Save the new customer to the repository
            await _customerRepository.SaveAsync(newCustomer, ct);

            return newCustomer;
        }
    }
}