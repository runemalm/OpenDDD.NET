using Bookstore.Domain.Model;
using OpenDDD.Infrastructure.Repository.Postgres.Base;

namespace Bookstore.Infrastructure.Repositories.Postgres
{
    public class PostgresSessionRepository : PostgresRepositoryBase<Customer, Guid>, ICustomerRepository
    {
        public PostgresSessionRepository() : base()
        {
            
        }

        public Customer GetByEmail(string email, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }
}
