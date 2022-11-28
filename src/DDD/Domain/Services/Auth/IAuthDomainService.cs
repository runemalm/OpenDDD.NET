using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DDD.Application;

namespace DDD.Domain.Services.Auth
{
	public interface IAuthDomainService : IDomainService
	{
		// Any mode
		Task AssureAuthenticatedAsync(ActionId actionId, CancellationToken ct);
		
		// RBAC mode
		Task AssurePermissionsInWorldAsync(string domainName, IEnumerable<string> permissions, ActionId actionId, CancellationToken ct);
		Task AssurePermissionsInRealmAsync(string realmId, string domainName, IEnumerable<string> permissions, ActionId actionId, CancellationToken ct);
		Task AssurePermissionsInResourceGroupAsync(string resourceGroupId, string domainName, IEnumerable<string> permissions, ActionId actionId, CancellationToken ct);
		Task AssurePermissionsInResourceAsync(string resourceId, string domainName, IEnumerable<string> permissions, ActionId actionId, CancellationToken ct);

		// Role-names-in-token mode
		Task AssureRolesInTokenAsync(IEnumerable<IEnumerable<string>> roles, ActionId actionId, CancellationToken ct);
	}
}
