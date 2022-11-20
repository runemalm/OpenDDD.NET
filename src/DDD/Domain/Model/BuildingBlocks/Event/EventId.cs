using System;

namespace DDD.Domain.Model.BuildingBlocks.Event
{
	public class EventId : IEquatable<EventId>
	{
		public string Value;
		public EventId() { }
		
		public static EventId Create()
		{
			var eventId = new EventId()
			{
				Value = Guid.NewGuid().ToString()
			};
			return eventId;
		}

		public override string ToString()
		{
			return Value;
		}
		
		// Equality
		
		public bool Equals(EventId other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Value == other.Value;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((EventId)obj);
		}

		public override int GetHashCode()
		{
			return (Value != null ? Value.GetHashCode() : 0);
		}

		public static bool operator ==(EventId left, EventId right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(EventId left, EventId right)
		{
			return !Equals(left, right);
		}
	}
}
