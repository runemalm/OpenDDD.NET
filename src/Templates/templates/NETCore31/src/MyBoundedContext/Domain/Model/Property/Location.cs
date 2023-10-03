using OpenDDD.Domain.Model.ValueObject;

namespace MyBoundedContext.Domain.Model.Property
{
    public class Location : ValueObject
    {
        public string Label { get; set; }
        public double Lat { get; set; }
        public double Lng { get; set; }
        
        public static Location Create(string label, double lat, double lng)
        {
            var location = 
                new Location
                {
                    Label = label,
                    Lat = lat,
                    Lng = lng
                };

            return location;
        }
    }
}
