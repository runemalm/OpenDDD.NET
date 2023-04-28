using System;
using OpenDDD.Application;
using OpenDDD.Domain.Model.BuildingBlocks.Event;
using ContextDomainModelVersion = Interchange.Domain.Model.DomainModelVersion;

namespace Interchange.Domain.Model.Forecast
{
    public class IcWeatherPredicted : IntegrationEvent, IEquatable<IcWeatherPredicted>
    {
        public string ForecastId { get; set; }
        public DateTime Date { get; set; }
        public int TemperatureC { get; set; }
        public string SummaryId { get; set; }
        
        public IcWeatherPredicted() { }

        public IcWeatherPredicted(ActionId actionId) : base("WeatherPredicted", ContextDomainModelVersion.Latest(), "Weather", actionId) { }

        public IcWeatherPredicted(IcForecast forecast, ActionId actionId) 
            : base("WeatherPredicted", ContextDomainModelVersion.Latest(), "Interchange", actionId)
        {
            ForecastId = forecast.ForecastId;
            Date = forecast.Date;
            TemperatureC = forecast.TemperatureC;
            SummaryId = forecast.SummaryId;
        }

        // Equality

        public bool Equals(IcWeatherPredicted other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && ForecastId == other.ForecastId && Date.Equals(other.Date) && TemperatureC == other.TemperatureC && SummaryId == other.SummaryId;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((IcWeatherPredicted)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), ForecastId, Date, TemperatureC, SummaryId);
        }
    }
}
