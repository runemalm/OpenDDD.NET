using System;
using System.Collections.Generic;
using OpenDDD.Application;
using OpenDDD.NET;

namespace OpenDDD.Domain.Model.BuildingBlocks.Event
{
	public class Event : IEvent, IEquatable<Event>
	{
		public EventHeader Header { get; set; }

		public Event() { }

		public Event(
			string eventName, 
			DomainModelVersion domainModelVersion, 
			string contextName,
			ActionId actionId,
			IEnumerable<string> corrIds,
			string actorId,
			string actorName,
			IDateTimeProvider dateTimeProvider)
		{
			var eventType = 
				contextName.ToLower() == "interchange" 
					? EventType.IntegrationEvent 
					: EventType.DomainEvent;

			Header = 
				new EventHeader(
					eventName, 
					contextName, 
					dateTimeProvider.Now,
					eventType, 
					domainModelVersion,
					actionId,
					EventId.Create(),
					corrIds,
					actorId,
					actorName);
		}

		public void AddDeliveryFailure(string error, IDateTimeProvider dateTimeProvider)
		{
			Header.AddDeliveryFailure(error, dateTimeProvider);
		}

		// Equality
		
		public bool Equals(Event other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Equals(Header, other.Header);
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
			return (Header != null ? Header.GetHashCode() : 0);
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
