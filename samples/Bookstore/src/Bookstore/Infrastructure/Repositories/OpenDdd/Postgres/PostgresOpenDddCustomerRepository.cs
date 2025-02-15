using OpenDDD.Infrastructure.Persistence.OpenDdd.DatabaseSession.Postgres;
using OpenDDD.Infrastructure.Repository.OpenDdd.Postgres;
using OpenDDD.Infrastructure.Persistence.Serializers;
using Bookstore.Domain.Model;

namespace Bookstore.Infrastructure.Repositories.OpenDdd.Postgres
{
    public class PostgresOpenDddCustomerRepository : PostgresOpenDddRepository<Customer, Guid>, ICustomerRepository
    {
        private readonly ILogger<PostgresOpenDddCustomerRepository> _logger;

        public PostgresOpenDddCustomerRepository(
            PostgresDatabaseSession session, 
            IAggregateSerializer serializer, 
            ILogger<PostgresOpenDddCustomerRepository> logger) 
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

            return (await FindWithAsync(c => c.Email == email, ct)).FirstOrDefault();
        }
    }
}
