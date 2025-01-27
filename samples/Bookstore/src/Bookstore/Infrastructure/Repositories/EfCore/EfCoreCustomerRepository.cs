using Microsoft.EntityFrameworkCore;
using OpenDDD.Infrastructure.Persistence.UoW;
using OpenDDD.Infrastructure.Repository.EfCore;
using Bookstore.Domain.Model;

namespace Bookstore.Infrastructure.Repositories.EfCore
{
    public class EfCoreCustomerRepository : EfCoreRepository<Customer, Guid>, ICustomerRepository
    {
        private readonly ILogger<EfCoreCustomerRepository> _logger;

        public EfCoreCustomerRepository(IUnitOfWork unitOfWork, ILogger<EfCoreCustomerRepository> logger) 
            : base(unitOfWork)
        {
            _logger = logger;
        }

        public async Task<Customer> GetByEmailAsync(string email, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Email cannot be null or whitespace.", nameof(email));
            }

            try
            {
                return await DbContext.Set<Customer>()
                           .FirstOrDefaultAsync(c => EF.Functions.Like(c.Email, email), cancellationToken: ct) 
                       ?? throw new KeyNotFoundException($"No customer found with email '{email}'.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while retrieving customer by email: {Email}", email);
                throw;
            }
        }
        
        public async Task<Customer?> FindByEmailAsync(string email, CancellationToken ct)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new ArgumentException("Email cannot be null or whitespace.", nameof(email));
            }

            return await DbContext.Set<Customer>()
                .FirstOrDefaultAsync(c => EF.Functions.Like(c.Email, email), cancellationToken: ct);
        }
    }
}
