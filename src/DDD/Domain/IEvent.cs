using System;

namespace DDD.Domain
{
	public interface IEvent : IBuildingBlock
	{
		public EventId EventId { get; }
		public ActionId ActionId { get; }
		DateTime OccuredAt { get; }
		string ActorId { get; }
		string ActorName { get; }
		string ContextName { get; }
		string EventName { get; }
	}
}
