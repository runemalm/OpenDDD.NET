using DDD.Domain;
using IcDomainModelVersion = Interchange.Domain.Model.DomainModelVersion;

namespace Interchange.Domain.Model.Forecast
{
    public class IcWeatherPredicted : IntegrationEvent, IEquatable<IcWeatherPredicted>
    {
        public string ForecastId { get; set; }
        public DateTime Date { get; set; }
        public int TemperatureC { get; set; }
        public string Summary { get; set; }
        
        public IcWeatherPredicted() { }

        public IcWeatherPredicted(ActionId actionId) : base("WeatherPredicted", IcDomainModelVersion.Latest(), "Weather", actionId) { }

        public IcWeatherPredicted(IcForecast forecast, ActionId actionId) 
            : base("WeatherPredicted", IcDomainModelVersion.Latest(), "Interchange", actionId)
        {
            ForecastId = forecast.ForecastId;
            Date = forecast.Date;
            TemperatureC = forecast.TemperatureC;
            Summary = forecast.Summary;
        }

        // Equality

        public bool Equals(IcWeatherPredicted? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && ForecastId == other.ForecastId && Date.Equals(other.Date) && TemperatureC == other.TemperatureC && Summary == other.Summary;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((IcWeatherPredicted)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), ForecastId, Date, TemperatureC, Summary);
        }

        public static bool operator ==(IcWeatherPredicted? left, IcWeatherPredicted? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(IcWeatherPredicted? left, IcWeatherPredicted? right)
        {
            return !Equals(left, right);
        }
    }
}
