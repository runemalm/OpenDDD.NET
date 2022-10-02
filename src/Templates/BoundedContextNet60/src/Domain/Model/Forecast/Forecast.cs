using DDD.Domain;
using DDD.Domain.Exceptions;
using DDD.Domain.Validation;
using DDD.Infrastructure.Ports;
using Interchange.Domain.Model.Forecast;

namespace Domain.Model.Forecast
{
    public class Forecast : Aggregate, IAggregate, IEquatable<Forecast>
    {
        private static readonly string[] Summaries = new[]
        {
            "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
        };
        
        public ForecastId ForecastId { get; set; }
        EntityId IAggregate.Id => ForecastId;
        
        public DateTime Date { get; set; }
        public int TemperatureC { get; set; }
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
        public string Summary { get; set; }

        // Public

        public static async Task<Forecast> PredictTomorrow(
            ForecastId forecastId, 
            ActionId actionId,
            IDomainPublisher domainPublisher,
            IInterchangePublisher interchangePublisher,
            IIcForecastTranslator icForecastTranslator)
        {
            var forecast =
                new Forecast()
                {
                    DomainModelVersion = Domain.Model.DomainModelVersion.Latest(),
                    ForecastId = forecastId,
                    Date = DateTime.Now.AddDays(1),
                    TemperatureC = Random.Shared.Next(-20, 55),
                    Summary = Summaries[Random.Shared.Next(Summaries.Length)]
                };

            forecast.Validate();
            
            await domainPublisher.PublishAsync(new WeatherPredicted(forecast, actionId));
            await interchangePublisher.PublishAsync(new IcWeatherPredicted(icForecastTranslator.To(forecast), actionId));

            return forecast;
        }

        // Private

        protected void Validate()
        {
            var validator = new Validator<Forecast>(this);

            var errors = validator
                .NotNull(bb => bb.ForecastId.Value)
                .NotNull(bb => bb.Date)
                .NotNull(bb => bb.TemperatureC)
                .Errors()
                .ToList();

            if (errors.Any())
            {
                throw new InvariantException(
                    $"Forecast is invalid with errors: " +
                    $"{string.Join(", ", errors.Select(e => $"{e.Key} {e.Details}"))}");
            }
        }

        // Equality

        public bool Equals(Forecast? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && ForecastId.Equals(other.ForecastId) && Date.Equals(other.Date) && TemperatureC == other.TemperatureC && Summary == other.Summary;
        }

        public override bool Equals(object? obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((Forecast)obj);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(base.GetHashCode(), ForecastId, Date, TemperatureC, Summary);
        }

        public static bool operator ==(Forecast? left, Forecast? right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(Forecast? left, Forecast? right)
        {
            return !Equals(left, right);
        }
    }
}
