using OpenDDD.Domain.Model;

namespace Bookstore.Domain.Model
{
	public interface IOrderRepository : IRepository<Order, Guid>
	{
		public IEnumerable<Order> FindByCustomer(Guid customerId, CancellationToken ct = default);
	}
}
