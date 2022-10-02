using DDD.Domain;

namespace Domain.Model.Forecast
{
    public class ForecastId : EntityId
    {
        public ForecastId(string value) : base(value) { }

        public static ForecastId Create(string value)
            => Create(DomainModelVersion.Latest(), value);

        public static ForecastId Create(DomainModelVersion domainModelVersion, string value)
        {
            var forecastId = new ForecastId(value);
            forecastId.Validate(nameof(forecastId));
            return forecastId;
        }
    }
}
