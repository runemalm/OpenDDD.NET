﻿using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using OpenDDD.Application;
using OpenDDD.Domain.Model.BuildingBlocks.Event;

namespace OpenDDD.Infrastructure.Ports.PubSub
{
	public interface IOutbox
	{
		Task AddAsync(ActionId actionId, IEvent theEvent, CancellationToken ct);
		Task AddAllAsync(ActionId actionId, IEnumerable<IEvent> events, CancellationToken ct);
		Task<OutboxEvent?> GetNextAsync(CancellationToken ct);
		Task MarkAsNotPublishingAsync(string id, CancellationToken ct);
		Task<IEnumerable<OutboxEvent>> GetAllAsync(ActionId actionId, CancellationToken ct);
		Task RemoveAsync(string id, ActionId actionId, CancellationToken ct);
		Task RemoveAsync(string id, CancellationToken ct);
		void Empty(CancellationToken ct);
		Task EmptyAsync(CancellationToken ct);
		void Start(CancellationToken ct);
		Task StartAsync(CancellationToken ct);
		void Stop(CancellationToken ct);
		Task StopAsync(CancellationToken ct);
	}
}