using DDD.Domain.Model.BuildingBlocks.Entity;

namespace Domain.Model.Forecast
{
    public class ForecastId : EntityId
    {
        public ForecastId(string value) : base(value) { }

        public static ForecastId Create(string value)
        {
            var forecastId = new ForecastId(value);
            forecastId.Validate(nameof(forecastId));
            return forecastId;
        }
    }
}
