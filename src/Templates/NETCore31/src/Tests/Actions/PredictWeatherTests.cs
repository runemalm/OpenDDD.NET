using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Application.Actions.Commands;
using Domain.Model.Forecast;
using Interchange.Domain.Model.Forecast;

namespace Tests.Actions
{
    public class PredictWeatherTests : ActionUnitTests
    {
        public PredictWeatherTests()
        {
            Configure();
            EmptyDb();
        }
        
        [Fact]
        public async Task TestSuccess_PredictionMade()
        {
            // Arrange
            await EnsureSummariesAsync();
    
            // Act
            var command = new PredictWeatherCommand()
            {
                
            };
    
            var forecast = await PredictWeatherAction.ExecuteAsync(command, ActionId, CancellationToken.None);
    
            // Assert
            AssertNow(forecast.Date);
            Assert.Contains(forecast.TemperatureC, Enumerable.Range(-20, 75));
            Assert.Equal(32 + (int)(forecast.TemperatureC / 0.5556), forecast.TemperatureF);
            Assert.NotNull(forecast.SummaryId);
        }
        
        [Fact]
        public async Task TestSuccess_EventsPublished()
        {
            // Arrange
            await EnsureSummariesAsync();
    
            // Act
            var command = new PredictWeatherCommand()
            {
                
            };
    
            var forecast = await PredictWeatherAction.ExecuteAsync(command, ActionId, CancellationToken.None);
    
            // Assert
            var expectedDomainEvent = new WeatherPredicted(forecast, ActionId);
            var expectedIntegrationEvent = new IcWeatherPredicted(
                new IcForecast(
                    forecast.ForecastId.ToString(),
                    forecast.Date,
                    forecast.TemperatureC,
                    forecast.SummaryId.ToString()), ActionId);
            
            AssertDomainEventPublished(expectedDomainEvent);
            AssertIntegrationEventPublished(expectedIntegrationEvent);
        }
    }
}
