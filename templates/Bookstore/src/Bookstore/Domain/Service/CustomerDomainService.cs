using OpenDDD.Domain.Model;
using OpenDDD.Domain.Model.Exception;
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
            _customerRepository = customerRepository;
            _domainPublisher = domainPublisher;
        }

        public async Task<Customer> RegisterAsync(string name, string email, CancellationToken ct)
        {
            var existingCustomer = await _customerRepository.FindByEmailAsync(email, ct);

            if (existingCustomer != null)
                throw new EntityExistsException("Customer", $"email '{email}'");

            var newCustomer = Customer.Create(name, email);

            await _customerRepository.SaveAsync(newCustomer, ct);

            var domainEvent = new CustomerRegistered(newCustomer.Id, newCustomer.Name, newCustomer.Email, DateTime.UtcNow);
            await _domainPublisher.PublishAsync(domainEvent, ct);

            return newCustomer;
        }
    }
}
