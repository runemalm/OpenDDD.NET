using OpenDDD.Domain.Model;

namespace Bookstore.Domain.Model
{
	public interface ICustomerRepository : IRepository<Customer, Guid>
	{
		public Task<Customer> GetByEmailAsync(string email, CancellationToken ct);
		public Task<Customer?> FindByEmailAsync(string email, CancellationToken ct);
	}
}
