using System.Threading.Tasks;
using Xunit;

namespace Tests.Domain.Model.Forecast
{
    public class ForecastDomainServiceTests : ActionUnitTests
    {
        public ForecastDomainServiceTests()
        {
            Configure();
            EmptyDb();
        }

        [Fact (Skip = "This is where you would implement your domain service tests.")]
        public Task TestSomethingAsync()
        {
            return Task.CompletedTask;
        }
    }
}
