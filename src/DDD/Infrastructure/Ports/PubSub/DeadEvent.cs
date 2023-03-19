using System;
using Newtonsoft.Json;
using DDD.Domain;
using DDD.Domain.Model;
using DDD.Domain.Model.BuildingBlocks;
using DDD.Domain.Model.BuildingBlocks.Event;
using DDD.Infrastructure.Ports.Adapters.Common.Translation.Converters;

namespace DDD.Infrastructure.Ports.PubSub
{
	public class DeadEvent : IEquatable<DeadEvent>
	{
		public string Id { get; set; }
		public EventId EventId { get; set; }
		public string JsonPayload { get; set; }

		public DeadEvent() { }

		public DeadEvent(IEvent theEvent, SerializerSettings serializerSettings)
		{
			Id = Guid.NewGuid().ToString();
			EventId = theEvent.Header.EventId;
			JsonPayload = JsonConvert.SerializeObject(theEvent, serializerSettings);
		}
		
		// Equality

		public bool Equals(DeadEvent other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Equals(EventId, other.EventId) && JsonPayload == other.JsonPayload;
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((DeadEvent)obj);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(EventId, JsonPayload);
		}

		public static bool operator ==(DeadEvent left, DeadEvent right)
		{
			return Equals(left, right);
		}

		public static bool operator !=(DeadEvent left, DeadEvent right)
		{
			return !Equals(left, right);
		}
	}
}
