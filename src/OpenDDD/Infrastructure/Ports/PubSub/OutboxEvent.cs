using System;
using OpenDDD.Domain;
using OpenDDD.Domain.Model.BuildingBlocks;
using Newtonsoft.Json;
using OpenDDD.Application;
using OpenDDD.Domain.Model;
using OpenDDD.Domain.Model.BuildingBlocks.Event;
using OpenDDD.Infrastructure.Ports.Adapters.Common.Translation.Converters;

namespace OpenDDD.Infrastructure.Ports.PubSub
{
	public class OutboxEvent : IEquatable<OutboxEvent>
	{
		public string Id { get; set; }
		public EventId EventId { get; set; }
		public ActionId ActionId { get; set; }
		public string EventName { get; set; }
		public DomainModelVersion DomainModelVersion { get; set; }
		public bool IsDomainEvent { get; set; }
		public DateTime AddedAt { get; set; }
		public bool IsPublishing { get; set; }
		public int NumDeliveryFailures { get; set; }
		public string JsonPayload { get; set; }

		public OutboxEvent() { }

		public OutboxEvent(IEvent theEvent, SerializerSettings serializerSettings)
		{
			Id = Guid.NewGuid().ToString();
			EventId = theEvent.Header.EventId;
			ActionId = theEvent.Header.ActionId;
			EventName = theEvent.Header.Name;
			DomainModelVersion = theEvent.Header.DomainModelVersion;
			IsDomainEvent = theEvent.Header.EventType == EventType.DomainEvent;
			AddedAt = DateTime.UtcNow;
			IsPublishing = false;
			NumDeliveryFailures = theEvent.Header.NumDeliveryFailures;
			JsonPayload = JsonConvert.SerializeObject(theEvent, serializerSettings);
		}
		
		// Equality

		public bool Equals(OutboxEvent other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return AddedAt.Equals(other.AddedAt) && Equals(EventId, other.EventId) && Equals(ActionId, other.ActionId) && EventName == other.EventName && Equals(DomainModelVersion, other.DomainModelVersion) && IsDomainEvent == other.IsDomainEvent && IsPublishing == other.IsPublishing && JsonPayload == other.JsonPayload;
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
			return HashCode.Combine(AddedAt, EventId, ActionId, EventName, DomainModelVersion, IsDomainEvent, IsPublishing, JsonPayload);
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
