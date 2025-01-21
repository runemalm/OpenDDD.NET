using Bookstore.Domain.Model;
using OpenDDD.Domain.Service;

namespace Bookstore.Domain.Services
{
    public interface ICustomerDomainService : IDomainService
    {
        Task<Customer> Register(string name, string email);
    }
}
