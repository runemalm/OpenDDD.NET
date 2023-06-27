using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting.Server.Features;
using OpenDDD.Application;
using OpenDDD.Domain.Model.BuildingBlocks.Event;
using OpenDDD.Infrastructure.Ports.Adapters.Common.Translation.Converters;
using OpenDDD.Infrastructure.Ports.PubSub;
using OpenDDD.NET;

namespace OpenDDD.Infrastructure.Ports.Adapters.PubSub.Memory
{
	public class MemoryOutbox : IOutbox
	{
		private IDictionary<ActionId, ICollection<OutboxEvent>> _events;
		private object _lock = new{};
		private readonly ConversionSettings _conversionSettings;
		private readonly IDateTimeProvider _dateTimeProvider;
		
		public MemoryOutbox(ConversionSettings conversionSettings, IDateTimeProvider dateTimeProvider)
		{
			_events = new Dictionary<ActionId, ICollection<OutboxEvent>>();
			_conversionSettings = conversionSettings;
			_dateTimeProvider = dateTimeProvider;
		}
		
		public void Start(CancellationToken ct)
		{
			
		}

		public Task StartAsync(CancellationToken ct)
		{
			return Task.CompletedTask;
		}

		public void Stop(CancellationToken ct)
		{
			
		}

		public Task StopAsync(CancellationToken ct)
		{
			return Task.CompletedTask;
		}
		
		public async Task AddAsync(ActionId actionId, IEvent theEvent, CancellationToken ct)
			=> AddAllAsync(actionId, new List<IEvent>{theEvent}, ct);

		public Task AddAllAsync(ActionId actionId, IEnumerable<IEvent> events, CancellationToken ct)
		{
			lock (_events)
			{
				if (!_events.ContainsKey(actionId))
					_events.Add(actionId, new List<OutboxEvent>());
				var es = _events[actionId].ToList();
				es.AddRange(events.Select(e => OutboxEvent.Create(e, _conversionSettings, _dateTimeProvider)));
				_events[actionId] = es;
				return Task.CompletedTask;
			}
		}
		
		public Task<OutboxEvent?> GetNextAsync(CancellationToken ct)
		{
			lock (_events)
			{
				var outBoxEvents = new List<OutboxEvent>();

				foreach (var list in _events.Values)
					outBoxEvents = outBoxEvents.Concat(list).ToList();

				var next = 
					outBoxEvents
						.Where(o => !o.IsPublishing)
						.OrderBy(o => o.AddedAt)
						.ToList()
						.FirstOrDefault();
				
				if (next != null)
					next.IsPublishing = true;

				return Task.FromResult(next);
			}
		}

		public async Task MarkAsNotPublishingAsync(string id, CancellationToken ct)
		{
			lock (_events)
			{
				foreach (var actionId in _events.Keys.ToList())
				{
					var outboxEvent = _events[actionId].FirstOrDefault(o => o.Id == id);
					if (outboxEvent != null)
						outboxEvent.IsPublishing = false;
				}
			}
		}

		public Task<IEnumerable<OutboxEvent>> GetAllAsync(ActionId actionId, CancellationToken ct)
		{
			var events = new List<OutboxEvent>();
			
			if (_events.ContainsKey(actionId))
				events = _events[actionId].ToList();

			return Task.FromResult(events.AsEnumerable());
		}

		public Task RemoveAsync(string id, ActionId actionId, CancellationToken ct)
		{
			lock (_events)
			{
				if (_events.ContainsKey(actionId))
					_events[actionId] = _events[actionId].Where(o => o.Id != id).ToList();
				return Task.CompletedTask;
			}
		}
		
		public Task RemoveAsync(string id, CancellationToken ct)
		{
			lock (_events)
			{
				foreach (var actionId in _events.Keys.ToList())
					_events[actionId] = _events[actionId].Where(o => o.Id != id).ToList();
			}
			return Task.CompletedTask;
		}

		public void Empty(CancellationToken ct)
		{
			lock (_events)
			{
				_events = new Dictionary<ActionId, ICollection<OutboxEvent>>();
			}
		}

		public Task EmptyAsync(CancellationToken ct)
		{
			Empty(ct);
			return Task.CompletedTask;
		}
	}
}
