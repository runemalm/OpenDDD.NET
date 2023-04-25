using Xunit;

namespace Tests.Domain.Model.Forecast;

public class AuthDomainServiceTests : ActionUnitTests
{
    public AuthDomainServiceTests()
    {
        Configure();
        EmptyDb();
    }

    [Fact]
    public async Task TestSomethingAsync()
    {
        throw new NotImplementedException();
    }
}
