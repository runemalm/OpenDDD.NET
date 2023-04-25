using Xunit;

namespace Tests.Actions;

public class NotifyWeatherPredictedTests : ActionUnitTests
{
    public NotifyWeatherPredictedTests()
    {
        Configure();
        EmptyDb();
    }
    
    [Fact]
    public async Task TestSuccess()
    {
        throw new NotImplementedException();
    }
}
