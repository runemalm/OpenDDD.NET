using System.ComponentModel;
using OpenDDD.Domain.Model.Entity;

namespace MyBoundedContext.Domain.Model.Property
{
    public class PropertyId : EntityId
    {
        public enum Type
        {
            [Description("Rental")]
            Rental
        }

        public PropertyId(string value) : base(value) { }

        public static PropertyId Create(string value)
        {
            var propertyId = new PropertyId(value);
            propertyId.Validate(nameof(propertyId));
            return propertyId;
        }
    }
}
