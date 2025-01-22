using Bookstore.Domain.Model;
using OpenDDD.Infrastructure.Repository.Postgres.Base;

namespace Bookstore.Infrastructure.Repositories.Postgres
{
    public class PostgresCustomerRepository : PostgresRepositoryBase<Customer, Guid>, ICustomerRepository
    {
        public PostgresCustomerRepository(ILogger<PostgresCustomerRepository> logger) : base(logger)
        {
            
        }

        public Customer GetByEmail(string email, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
