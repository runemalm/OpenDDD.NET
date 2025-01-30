using OpenDDD.Domain.Service;
using Bookstore.Domain.Model;

namespace Bookstore.Domain.Service
{
    public interface ICustomerDomainService : IDomainService
    {
        Task<Customer> RegisterAsync(string name, string email, CancellationToken ct);
    }
}
