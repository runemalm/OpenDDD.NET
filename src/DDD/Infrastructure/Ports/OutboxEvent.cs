using System;
using DDD.Domain;
using Newtonsoft.Json;

namespace DDD.Infrastructure.Ports
{
	public class OutboxEvent : IEquatable<OutboxEvent>
	{
		public EventId EventId { get; set; }
		public ActionId ActionId { get; set; }
		public string EventName { get; set; }
		public DomainModelVersion DomainModelVersion { get; set; }
		public bool IsDomainEvent { get; set; }
		public string JsonPayload { get; set; }

		public OutboxEvent() { }

		public OutboxEvent(IEvent theEvent)
		{
			IsDomainEvent = theEvent.GetType().IsSubclassOf(typeof(DomainEvent));
			EventId = theEvent.EventId;
			ActionId = theEvent.ActionId;
			EventName = theEvent.EventName;
			DomainModelVersion = theEvent.DomainModelVersion;
			JsonPayload = JsonConvert.SerializeObject(theEvent);
		}
		
		// Equality

		public bool Equals(OutboxEvent other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Equals(EventId, other.EventId) && Equals(ActionId, other.ActionId) && EventName == other.EventName && Equals(DomainModelVersion, other.DomainModelVersion) && IsDomainEvent == other.IsDomainEvent && JsonPayload == other.JsonPayload;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((OutboxEvent)obj);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(EventId, ActionId, EventName, DomainModelVersion, IsDomainEvent, JsonPayload);
		}

		public static bool operator ==(OutboxEvent left, OutboxEvent right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(OutboxEvent left, OutboxEvent right)
		{
			return !Equals(left, right);
		}
	}
}
