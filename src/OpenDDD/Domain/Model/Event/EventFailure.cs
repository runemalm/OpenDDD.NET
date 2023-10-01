using System;
using OpenDDD.NET;

namespace OpenDDD.Domain.Model.Event
{
	public class EventFailure : IEquatable<EventFailure>
	{
		public DateTime DateTime { get; set; }
		public string Message { get; set; }

		public EventFailure() { }

		public EventFailure(string message, IDateTimeProvider dateTimeProvider)
		{
			DateTime = dateTimeProvider.Now;
			Message = message;
		}

		public static EventFailure Create(string message, IDateTimeProvider dateTimeProvider)
		{
			var eventFailure = new EventFailure(message, dateTimeProvider);
			return eventFailure;
		}
		
		// Equality
		
		public bool Equals(EventFailure other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return DateTime.Equals(other.DateTime) && Message == other.Message;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((EventFailure)obj);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(DateTime, Message);
		}

		public static bool operator ==(EventFailure left, EventFailure right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(EventFailure left, EventFailure right)
		{
			return !Equals(left, right);
		}
	}
}
