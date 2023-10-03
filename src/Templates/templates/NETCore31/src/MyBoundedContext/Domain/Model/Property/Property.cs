using System;
using OpenDDD.Application;
using OpenDDD.Domain.Model.AggregateRoot;
using OpenDDD.Domain.Model.Entity;
using AppDomainModelVersion = MyBoundedContext.Domain.Model.DomainModelVersion;

namespace MyBoundedContext.Domain.Model.Property
{
    public class Property : AggregateRoot, IAggregateRoot, IEquatable<Property>
    {
        public PropertyId PropertyId { get; set; }
        EntityId IAggregateRoot.Id => PropertyId;
        
        public string Title { get; set; }
        public string Description { get; set; }
        public Price Price { get; set; }
        public Location Location { get; set; }
        public PropertyType PropertyType { get; set; }
        // public ApartmentNumber ApartmentNumber { get; set; }
        // public Size Size { get; set; }
        // public int Rooms { get; set; }
        // public ICollection<Amenity> Amenities { get; set; }

        // Public

        public static Property Create(
            PropertyId propertyId,
            string title,
            string description,
            Price price,
            Location location,
            PropertyType propertyType,
            ActionId actionId)
        {
            var property =
                new Property
                {
                    DomainModelVersion = AppDomainModelVersion.Latest(),
                    PropertyId = propertyId,
                    Title = title,
                    Description = description,
                    Price = price,
                    Location = location,
                    PropertyType = propertyType
                };

            return property;
        }

        // Private

        

        // Equality

        public bool Equals(Property other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && Equals(PropertyId, other.PropertyId) && Title == other.Title && Description == other.Description && Equals(Price, other.Price) && Equals(Location, other.Location) && PropertyType == other.PropertyType;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Property)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), PropertyId, Title, Description, Price, Location, (int)PropertyType);
        }

        public static bool operator ==(Property left, Property right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Property left, Property right)
        {
            return !Equals(left, right);
        }
    }
}
