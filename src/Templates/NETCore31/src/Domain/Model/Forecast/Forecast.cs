using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using OpenDDD.Application;
using OpenDDD.Domain.Model.BuildingBlocks.Aggregate;
using OpenDDD.Domain.Model.BuildingBlocks.Entity;
using OpenDDD.Domain.Model.Error;
using OpenDDD.Domain.Model.Validation;
using OpenDDD.Infrastructure.Ports.PubSub;
using Domain.Model.Summary;
using Interchange.Domain.Model.Forecast;
using WeatherDomainModelVersion = Domain.Model.DomainModelVersion;

namespace Domain.Model.Forecast
{
    public class Forecast : Aggregate, IAggregate, IEquatable<Forecast>
    {
        public ForecastId ForecastId { get; set; }
        EntityId IAggregate.Id => ForecastId;
        
        public DateTime Date { get; set; }
        public int TemperatureC { get; set; }
        public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
        public SummaryId SummaryId { get; set; }

        // Public

        public static async Task<Forecast> PredictTomorrowAsync(
            ForecastId forecastId, 
            ActionId actionId,
            ISummaryRepository summaryRepository,
            IDomainPublisher domainPublisher,
            IInterchangePublisher interchangePublisher,
            IIcForecastTranslator icForecastTranslator,
            CancellationToken ct)
        {
            var summaries = 
                (await summaryRepository
                    .GetAllAsync(actionId, ct))
                    .ToList();

            var summaryId =
                summaries[new Random().Next(summaries.Count)].SummaryId;
            
            var forecast =
                new Forecast()
                {
                    DomainModelVersion = WeatherDomainModelVersion.Latest(),
                    ForecastId = forecastId,
                    Date = DateTime.Now.AddDays(1),
                    TemperatureC = new Random().Next(-20, 55),
                    SummaryId = summaryId
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
                throw DomainException.InvariantViolation(
                    $"Forecast is invalid with errors: " +
                    $"{string.Join(", ", errors.Select(e => $"{e.Key} {e.Details}"))}");
            }
        }

        // Equality


        public bool Equals(Forecast? other)
        {
            if (ReferenceEquals(null, other)) return false;
            if (ReferenceEquals(this, other)) return true;
            return base.Equals(other) && ForecastId.Equals(other.ForecastId) && Date.Equals(other.Date) && TemperatureC == other.TemperatureC && SummaryId.Equals(other.SummaryId);
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
            return HashCode.Combine(base.GetHashCode(), ForecastId, Date, TemperatureC, SummaryId);
        }
    }
}
