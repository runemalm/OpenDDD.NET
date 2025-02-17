using OpenDDD.Infrastructure.Persistence.OpenDdd.DatabaseSession.InMemory;
using OpenDDD.Infrastructure.Persistence.Serializers;
using OpenDDD.Infrastructure.Repository.OpenDdd.InMemory;
using Bookstore.Domain.Model;

namespace Bookstore.Infrastructure.Repositories.OpenDdd.InMemory
{
    public class InMemoryOpenDddCustomerRepository 
        : InMemoryOpenDddRepository<Customer, Guid>, ICustomerRepository
    {
        private readonly ILogger<InMemoryOpenDddCustomerRepository> _logger;

        public InMemoryOpenDddCustomerRepository(
            InMemoryDatabaseSession session, 
            IAggregateSerializer serializer,
            ILogger<InMemoryOpenDddCustomerRepository> logger)
            : base(session, serializer)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        public async Task<Customer> GetByEmailAsync(string email, CancellationToken ct = default)
        {
            var customer = await FindByEmailAsync(email, ct);
            return customer ?? throw new KeyNotFoundException($"No customer found with email '{email}'.");
        }

        public async Task<Customer?> FindByEmailAsync(string email, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Email cannot be null or whitespace.", nameof(email));
            }

            var allCustomers = await FindAllAsync(ct);
            var customer = allCustomers.FirstOrDefault(c => c.Email == email);

            _logger.LogDebug("FindByEmailAsync: {Email} -> {Found}", email, customer != null);
            return customer;
        }
    }
}
