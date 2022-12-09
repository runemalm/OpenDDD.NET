﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using DDD.Application;
using DDD.Infrastructure.Ports.Auth;

namespace DDD.Infrastructure.Ports.Adapters.Auth.IAM.Negative
{
	public class NegativeIamAdapter : IIamPort
	{
		public Task<bool> HasPermissionsInWorldAsync(string domain, IEnumerable<string> permissions, ActionId actionId, CancellationToken ct)
			=> Task.FromResult(false);

		public Task<bool> HasPermissionsInRealmAsync(string realmId, string externalRealmId, string domain, IEnumerable<string> permissions, ActionId actionId, CancellationToken ct)
			=> Task.FromResult(false);

		public Task<bool> HasPermissionsInResourceGroupAsync(string resourceGroupId, string domain, IEnumerable<string> permissions, ActionId actionId, CancellationToken ct)
			=> Task.FromResult(false);

		public Task<bool> HasPermissionsInResourceAsync(string resourceId, string domain, IEnumerable<string> permissions, ActionId actionId, CancellationToken ct)
			=> Task.FromResult(false);
	}
}
