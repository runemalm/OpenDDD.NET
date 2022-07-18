using System;
using WireMock.Server;
using Xunit;

namespace DDD.Tests
{
    [Collection("Sequential")]
    public class ActionUnitTests : UnitTests
    {
        // Configuration
        
        public void SetConfigPersistenceProvider(string value)
            => Environment.SetEnvironmentVariable("CFG_PERSISTENCE_PROVIDER", value);
        
        // Mock API
        
        private WireMockServer _mockApi;
        public WireMockServer MockApi
        {
            get
            {
                if (_mockApi == null)
                    _mockApi = WireMockServer.Start();
                return _mockApi;
            }
            set { }
        }
    }
}
