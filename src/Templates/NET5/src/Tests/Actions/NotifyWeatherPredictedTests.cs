using System.Threading;
using System.Threading.Tasks;
using Xunit;
using DDD.Infrastructure.Ports.PubSub;
using Application.Actions.Commands;
using Domain.Model.Forecast;
using Domain.Model.Notification;

namespace Tests.Actions
{
    public class NotifyWeatherPredictedTests : ActionUnitTests
    {
        public NotifyWeatherPredictedTests()
        {
            Configure();
            EmptyDb();
        }
        
        [Fact]
        public async Task TestSuccess_EmailSent()
        {
            // Arrange
            await EnsureSummariesAsync();
    
            await DoWithEmailDisabled(async () =>
            {
                await PredictWeather();
            });
    
            // Act
            var command = new NotifyWeatherPredictedCommand
            {
                ForecastId = Forecast.ForecastId,
                Date = Forecast.Date,
                SummaryId = Forecast.SummaryId,
                TemperatureC = Forecast.TemperatureC
            };
    
            await NotifyWeatherPredictedAction.ExecuteAsync(command, ActionId, CancellationToken.None);
    
            // Assert
            AssertEmailSent(toEmail: Email.Create("bob@example.com"), msgContains: $"the weather tomorrow will be ");
        }
        
        [Fact]
        public async Task TestSuccess_EmailSentOnWeatherPredictedDomainEvent()
        {
            // Arrange
            await EnsureSummariesAsync();
            
            DisableDomainEvents();
            await PredictWeather();
            EnableDomainEvents();
    
            // Act
            await DomainPublisher.FlushAsync(new OutboxEvent(new WeatherPredicted(Forecast, ActionId), SerializerSettings));
        
            // Assert
            AssertEmailSent(toEmail: Email.Create("bob@example.com"), msgContains: $"the weather tomorrow will be ");
        }
    }
}
