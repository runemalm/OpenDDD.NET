using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OpenDDD.Application;

namespace OpenDDD.Domain.Services.Auth
{
	public interface IAuthDomainService : IDomainService
	{
		// Authenticated
		Task AssureAuthenticatedAsync(ActionId actionId, CancellationToken ct);
		
		// Role names in token
		Task AssureRolesInTokenAsync(IEnumerable<IEnumerable<string>> roles, ActionId actionId, CancellationToken ct);
		
		// RBAC
		Task AssurePermissionsInWorldAsync(IEnumerable<(string, string)> permissions, ActionId actionId, CancellationToken ct);
		Task AssurePermissionsInRealmAsync(string? realmId, string? externalRealmId, IEnumerable<(string, string)> permissions, ActionId actionId, CancellationToken ct);
		Task AssurePermissionsInRealmAsync(string? realmId, string? externalRealmId, IEnumerable<(string, string)> permissions, string actorId, ActionId actionId, CancellationToken ct);
		Task AssurePermissionsInResourceGroupAsync(string resourceGroupId, string externalResourceGroupId, IEnumerable<(string, string)> permissions, ActionId actionId, CancellationToken ct);
		Task AssurePermissionsInResourceAsync(string resourceId, string externalResourceId, IEnumerable<(string, string)> permissions, ActionId actionId, CancellationToken ct);
	}
}
