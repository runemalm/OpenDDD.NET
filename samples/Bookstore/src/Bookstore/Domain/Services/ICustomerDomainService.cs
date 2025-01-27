using Bookstore.Domain.Model;
using OpenDDD.Domain.Service;

namespace Bookstore.Domain.Services
{
    public interface ICustomerDomainService : IDomainService
    {
        Task<Customer> RegisterAsync(string name, string email, CancellationToken ct);
    }
}
