using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace DDD.Domain.Auth
{
	public interface IAuthDomainService : IDomainService
	{
		Task AuthorizeRolesAsync(IEnumerable<IEnumerable<string>> roles, CancellationToken ct);
		void CheckRolesInToken(IEnumerable<IEnumerable<string>> roles);
	}
}
