using OpenDDD.Domain.Service;
using Bookstore.Domain.Model;

namespace Bookstore.Domain.Service
{
    public interface IOrderDomainService : IDomainService
    {
        Task<Order> PlaceOrderAsync(Guid customerId, Guid bookId, CancellationToken ct);
    }
}
