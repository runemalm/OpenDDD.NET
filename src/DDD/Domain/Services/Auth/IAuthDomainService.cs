using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DDD.Application;

namespace DDD.Domain.Services.Auth
{
	public interface IAuthDomainService : IDomainService
	{
		// Authenticated
		Task AssureAuthenticatedAsync(ActionId actionId, CancellationToken ct);
		
		// Role names in token
		Task AssureRolesInTokenAsync(IEnumerable<IEnumerable<string>> roles, ActionId actionId, CancellationToken ct);
		
		// RBAC
		Task AssurePermissionsInWorldAsync(string domain, IEnumerable<string> permissions, ActionId actionId, CancellationToken ct);
		Task AssurePermissionsInRealmAsync(string realmId, string externalRealmId, string domain, IEnumerable<string> permissions, ActionId actionId, CancellationToken ct);
		Task AssurePermissionsInResourceGroupAsync(string resourceGroupId, string domain, IEnumerable<string> permissions, ActionId actionId, CancellationToken ct);
		Task AssurePermissionsInResourceAsync(string resourceId, string domain, IEnumerable<string> permissions, ActionId actionId, CancellationToken ct);
	}
}
