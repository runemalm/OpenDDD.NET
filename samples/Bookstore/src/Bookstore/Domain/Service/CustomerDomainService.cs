using OpenDDD.Domain.Model;
using Bookstore.Domain.Model;
using Bookstore.Domain.Model.Events;

namespace Bookstore.Domain.Service
{
    public class CustomerDomainService : ICustomerDomainService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IDomainPublisher _domainPublisher;

        public CustomerDomainService(ICustomerRepository customerRepository, IDomainPublisher domainPublisher)
        {
            _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
            _domainPublisher = domainPublisher ?? throw new ArgumentNullException(nameof(domainPublisher));
        }

        public async Task<Customer> RegisterAsync(string name, string email, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Customer name cannot be empty.", nameof(name));

            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Customer email cannot be empty.", nameof(email));
            
            var existingCustomer = await _customerRepository.FindByEmailAsync(email, ct);

            if (existingCustomer != null)
                throw new InvalidOperationException($"A customer with the email '{email}' already exists.");

            var newCustomer = Customer.Create(name, email);

            await _customerRepository.SaveAsync(newCustomer, ct);

            var domainEvent = new CustomerRegistered(newCustomer.Id, newCustomer.Name, newCustomer.Email, DateTime.UtcNow);
            await _domainPublisher.PublishAsync(domainEvent, ct);

            return newCustomer;
        }
    }
}
