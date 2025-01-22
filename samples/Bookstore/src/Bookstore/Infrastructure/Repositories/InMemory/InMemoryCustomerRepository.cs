using Bookstore.Domain.Model;
using OpenDDD.Infrastructure.Repository.InMemory.Base;
using OpenDDD.Main.Attributes;

namespace Bookstore.Infrastructure.Repositories.InMemory
{
    [Lifetime(ServiceLifetime.Singleton)]
    public class InMemoryCustomerRepository : InMemoryRepositoryBase<Customer, Guid>, ICustomerRepository
    {
        public InMemoryCustomerRepository(ILogger<InMemoryCustomerRepository> logger) : base(logger)
        {
        }

        public Customer GetByEmail(string email, CancellationToken ct = default)
        {
            var customer = FindWithAsync(c => c.Email == email, ct).Result.FirstOrDefault();

            if (customer == null)
            {
                throw new KeyNotFoundException($"Customer with email {email} was not found.");
            }

            return customer;
        }
    }
}
