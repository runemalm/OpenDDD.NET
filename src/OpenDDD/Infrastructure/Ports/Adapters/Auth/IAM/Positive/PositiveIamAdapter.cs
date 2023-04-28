using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OpenDDD.Application;
using OpenDDD.Infrastructure.Ports.Auth;

namespace OpenDDD.Infrastructure.Ports.Adapters.Auth.IAM.Positive
{
	public class PositiveIamAdapter : IIamPort
	{
		public Task<bool> HasPermissionsInWorldAsync(IEnumerable<(string, string)> permissions, ActionId actionId, CancellationToken ct)
			=> Task.FromResult(true);

		public Task<bool> HasPermissionsInRealmAsync(string realmId, string externalRealmId, IEnumerable<(string, string)> permissions, ActionId actionId, CancellationToken ct)
			=> Task.FromResult(true);

		public Task<bool> HasPermissionsInResourceGroupAsync(string resourceGroupId, string externalResourceGroupId, IEnumerable<(string, string)> permissions, ActionId actionId, CancellationToken ct)
			=> Task.FromResult(true);

		public Task<bool> HasPermissionsInResourceAsync(string resourceId, string externalResourceId, IEnumerable<(string, string)> permissions, ActionId actionId, CancellationToken ct)
			=> Task.FromResult(true);
	}
}
