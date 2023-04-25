using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;
using Application.Actions.Commands;

namespace Tests.Actions
{
    public class GetAverageTemperatureTests : ActionUnitTests
    {
        public GetAverageTemperatureTests()
        {
            Configure();
            EmptyDb();
        }

        [Fact]
        public async Task TestSuccess_AverageReturned()
        {
            // Arrange
            await EnsureSummariesAsync();
        
            await PredictWeather();
            await PredictWeather();
            await PredictWeather();
        
            // Act
            var command = new GetAverageTemperatureCommand()
            {
            
            };

            int averageTemp = await GetAverageTemperatureAction.ExecuteAsync(command, ActionId, CancellationToken.None);

            // Assert
            Assert.Equal(Convert.ToInt32(Forecasts.Average(f => f.TemperatureC)), averageTemp);
        }
    }
}
