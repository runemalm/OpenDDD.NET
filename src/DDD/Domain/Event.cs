using System;

namespace DDD.Domain
{
	public class Event : IEvent, IEquatable<Event>
	{
		public EventId EventId { get; set; }
		public ActionId ActionId { get; set; }
		public DateTime OccuredAt { get; set; }
		public string ActorId { get; set; }
		public string ActorName { get; set; }
		public string ContextName { get; set; }
		public string EventName { get; set; }
		public DomainModelVersion DomainModelVersion { get; set; }

		public Event() { }

		public Event(string eventName, DomainModelVersion domainModelVersion, string contextName, ActionId actionId)
		{
			EventId = EventId.Create();
			ActionId = actionId;
			OccuredAt = DateTime.UtcNow;
			EventName = eventName;
			DomainModelVersion = domainModelVersion;
			ContextName = contextName;
		}
		
		// Equality
		
		public bool Equals(Event other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Equals(EventId, other.EventId) && Equals(ActionId, other.ActionId) && OccuredAt.Equals(other.OccuredAt) && ActorId == other.ActorId && ActorName == other.ActorName && ContextName == other.ContextName && EventName == other.EventName && Equals(DomainModelVersion, other.DomainModelVersion);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((Event)obj);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(EventId, ActionId, OccuredAt, ActorId, ActorName, ContextName, EventName, DomainModelVersion);
		}

		public static bool operator ==(Event left, Event right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(Event left, Event right)
		{
			return !Equals(left, right);
		}
	}
}
