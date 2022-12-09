using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DDD.Application;

namespace DDD.Infrastructure.Ports.Auth
{
	public interface IIamPort
	{
		Task<bool> HasPermissionsInWorldAsync(string domain, IEnumerable<string> permissions, ActionId actionId, CancellationToken ct);
		Task<bool> HasPermissionsInRealmAsync(string realmId, string externalRealmId, string domain, IEnumerable<string> permissions, ActionId actionId, CancellationToken ct);
		Task<bool> HasPermissionsInResourceGroupAsync(string resourceGroupId, string domain, IEnumerable<string> permissions, ActionId actionId, CancellationToken ct);
		Task<bool> HasPermissionsInResourceAsync(string resourceId, string domain, IEnumerable<string> permissions, ActionId actionId, CancellationToken ct);
	}
}
