using Bookstore.Domain.Model;

namespace Bookstore.Domain.Services
{
    public class CustomerDomainService : ICustomerDomainService
    {
        private readonly ICustomerRepository _customerRepository;

        public CustomerDomainService(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
        }

        public async Task<Customer> Register(string name, string email)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Customer name cannot be empty.", nameof(name));

            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Customer email cannot be empty.", nameof(email));

            // Check if a customer with the same email already exists
            var existingCustomer = await _customerRepository.FindWithAsync(
                c => c.Email.Equals(email, StringComparison.OrdinalIgnoreCase), 
                default);

            if (existingCustomer.Any())
                throw new InvalidOperationException($"A customer with the email '{email}' already exists.");

            // Create a new customer
            var newCustomer = new Customer(Guid.NewGuid(), name, email);

            // Save the new customer to the repository
            await _customerRepository.SaveAsync(newCustomer, default);

            return newCustomer;
        }
    }
}