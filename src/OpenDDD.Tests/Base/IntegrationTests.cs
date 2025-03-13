using Microsoft.Extensions.Logging;
using OpenDDD.Tests.Base.Logging;
using Xunit.Abstractions;

namespace OpenDDD.Tests.Base
{
    [Trait("Category", "Integration")]
    public class IntegrationTests : IDisposable
    {
        protected readonly ILoggerFactory LoggerFactory;
        protected readonly ILogger Logger;

        public IntegrationTests(ITestOutputHelper testOutputHelper, bool enableLogging = false)
        {
            LoggerFactory = Microsoft.Extensions.Logging.LoggerFactory.Create(builder =>
            {
                if (enableLogging)
                {
                    builder.AddProvider(new XunitLoggerProvider(testOutputHelper));
                    builder.SetMinimumLevel(LogLevel.Debug);
                }
            });

            Logger = LoggerFactory.CreateLogger(GetType());
        }

        public void Dispose()
        {
            LoggerFactory.Dispose();
        }
    }
}
