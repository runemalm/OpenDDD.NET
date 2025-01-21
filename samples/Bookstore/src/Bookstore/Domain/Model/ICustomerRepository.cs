using OpenDDD.Domain.Model;

namespace Bookstore.Domain.Model
{
	public interface ICustomerRepository : IRepository<Customer, Guid>
	{
		public Customer GetByEmail(string email, CancellationToken ct = default);
	}
}
