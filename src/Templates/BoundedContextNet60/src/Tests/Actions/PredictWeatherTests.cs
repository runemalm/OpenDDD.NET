using Xunit;
using Application.Actions.Commands;

namespace Tests.Actions;

public class PredictWeatherTests : ActionUnitTests
{
    public PredictWeatherTests()
    {
        Configure();
        EmptyDb();
    }
    
    [Fact]
    public async Task TestSuccess()
    {
        // Arrange
        await EnsureSummariesAsync();

        var command = new PredictWeatherCommand()
        {
            
        };
        
        // Act
        var forecast = await PredictWeatherAction.ExecuteAsync(command, ActionId, CancellationToken.None);

        // Assert
        AssertNow(forecast.Date);
        Assert.Contains(forecast.TemperatureC, Enumerable.Range(-20, 55));
        Assert.Equal(forecast.TemperatureC + 32, forecast.TemperatureF);
        Assert.NotNull(forecast.SummaryId);
    }
}
