using System;
using System.Collections.Generic;
using System.Linq;
using DDD.Application;

namespace DDD.Domain.Model.BuildingBlocks.Event
{
	public class EventHeader : IEquatable<EventHeader>
	{
		public string Name { get; set; }
		public string Context { get; set; }
		public DateTime OccuredAt { get; set; }
		public EventType EventType { get; set; }
		public DomainModelVersion DomainModelVersion { get; set; }
		public ActionId ActionId { get; set; }
		public EventId EventId { get; set; }
		public IEnumerable<string> CorrelationIds { get; set; }
		public ICollection<EventFailure> DeliveryFailures { get; set; }
		public ICollection<EventFailure> PublishFailures { get; set; }
		public string ActorId { get; set; }
		public string ActorName { get; set; }
		public int NumDeliveryFailures => !DeliveryFailures.Any() ? 0 : DeliveryFailures.Count();
		public int NumDeliveryRetries => !DeliveryFailures.Any() ? 0 : DeliveryFailures.Count() - 1;

		public EventHeader() { }

		public EventHeader(
			string name,
			string context, 
			DateTime occuredAt, 
			EventType eventType, 
			DomainModelVersion domainModelVersion, 
			ActionId actionId, 
			EventId eventId, 
			IEnumerable<string> corrIds,
			string actorId,
			string actorName)
		{
			Name = name;
			Context = context;
			OccuredAt = occuredAt;
			EventType = eventType;
			DomainModelVersion = domainModelVersion;
			ActionId = actionId;
			EventId = EventId.Create();
			CorrelationIds = corrIds;
			DeliveryFailures = new List<EventFailure>();
			PublishFailures = new List<EventFailure>();
			ActorId = actorId;
			ActorName = actorName;
		}

		public void AddDeliveryFailure(string error)
		{
			DeliveryFailures.Add(EventFailure.Create(error));
		}
		
		// Equality

		public bool Equals(EventHeader other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Name == other.Name && Context == other.Context && OccuredAt.Equals(other.OccuredAt) && EventType == other.EventType && Equals(DomainModelVersion, other.DomainModelVersion) && Equals(ActionId, other.ActionId) && Equals(EventId, other.EventId) && Equals(CorrelationIds, other.CorrelationIds) && Equals(DeliveryFailures, other.DeliveryFailures) && Equals(PublishFailures, other.PublishFailures) && ActorId == other.ActorId && ActorName == other.ActorName;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((EventHeader)obj);
		}

		public override int GetHashCode()
		{
			var hashCode = new HashCode();
			hashCode.Add(Name);
			hashCode.Add(Context);
			hashCode.Add(OccuredAt);
			hashCode.Add((int)EventType);
			hashCode.Add(DomainModelVersion);
			hashCode.Add(ActionId);
			hashCode.Add(EventId);
			hashCode.Add(CorrelationIds);
			hashCode.Add(DeliveryFailures);
			hashCode.Add(PublishFailures);
			hashCode.Add(ActorId);
			hashCode.Add(ActorName);
			return hashCode.ToHashCode();
		}

		public static bool operator ==(EventHeader left, EventHeader right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(EventHeader left, EventHeader right)
		{
			return !Equals(left, right);
		}
	}
}
