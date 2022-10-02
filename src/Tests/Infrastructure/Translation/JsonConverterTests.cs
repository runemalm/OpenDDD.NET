using System.Threading.Tasks;
using DDD.Tests;
using Xunit;

namespace Tests.Infrastructure.Translation
{
	public class JsonConverterTests : UnitTests
	{
		public JsonConverterTests() : base()
		{

		}

		private async Task CommonArrangeAsync()
		{
			
		}

		[Fact]
		public async Task Assert_converts_aggregates_correctly()
		{
			// // Arrange
			// await CommonArrangeAsync();
			//
			// var site = Site.Cre
			//
			// // Act
			// var command =
			// 	new GetMarketRulesCommand()
			// 	{
			// 		Market = Market.SWE
			// 	};
			//
			// var returned =
			// 	await GetMarketRulesAction.ExecuteAsync(
			// 		command,
			// 		CancellationToken.None);
			//
			// // Assert
			// Assert.Equal(sweRules, returned);
		}
	}
}
