using Xunit;

namespace Tests.Actions;

public class GetAverageTemperatureTests : ActionUnitTests
{
    public GetAverageTemperatureTests()
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
