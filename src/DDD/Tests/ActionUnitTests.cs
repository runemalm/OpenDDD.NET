using System;
using System.Threading;
using System.Threading.Tasks;
using DDD.Application.Settings;
using DDD.Domain;
using DDD.Domain.Auth.Exceptions;
using DDD.Infrastructure.Ports;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using WireMock.Server;
using Xunit;

namespace DDD.Tests
{
    [Collection("Sequential")]
    public abstract class ActionUnitTests : UnitTests
    {
        public ActionUnitTests(bool emptyRepositoriesBeforeTests = true)
        {
            if (emptyRepositoriesBeforeTests)
            {
                EmptyRepositories().Wait();
                EmptyDeadLetterQueue().Wait();
            }
        }

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

        // PubSub
        
        public IDomainPublisher DomainPublisher => TestServer.Host.Services.GetRequiredService<IDomainPublisher>();
        public IInterchangePublisher InterchangePublisher => TestServer.Host.Services.GetRequiredService<IInterchangePublisher>();
        
        // Repositories
        
        public IOutbox Outbox => TestServer.Host.Services.GetRequiredService<IOutbox>();
        public IDeadLetterQueue DeadLetterQueue => TestServer.Host.Services.GetRequiredService<IDeadLetterQueue>();
        
        // Settings
        
        public ISettings Settings => TestServer.Host.Services.GetRequiredService<ISettings>();
        
        // Test server
        
        private TestServer _testServer;
        public TestServer TestServer
        {
            get
            {
                if (_testServer == null)
                {
                    var builder = CreateWebHostBuilder();
                    _testServer = new TestServer(builder);
                }
                return _testServer;
            }
        }

        protected abstract IWebHostBuilder CreateWebHostBuilder();

        // Persistence
        
        protected async Task EmptyRepositories()
        {
            await Outbox.EmptyAsync(CancellationToken.None);
            await EmptyAggregateRepositories(CancellationToken.None);
        }
        
        protected async Task EmptyDeadLetterQueue()
        {
            await DeadLetterQueue.EmptyAsync(CancellationToken.None);
        }
        
        protected abstract Task EmptyAggregateRepositories(CancellationToken ct);

        // Assertions
        
        public void AssertDomainEventPublished(Event event_)
            => Assert.True(DomainPublisher.HasPublished(event_), $"Expected domain event to have been published: {event_.Header.Name}.");

        public void AssertIntegrationEventPublished(Event event_)
            => Assert.True(InterchangePublisher.HasPublished(event_), $"Expected integration event to have been published: {event_.Header.Name}.");

        public async Task AssertAllowedAsync<T>(bool shouldBeAllowed, Func<Task<T>> actionAsync)
        {
            if (shouldBeAllowed)
                await actionAsync();
            else
                Assert.ThrowsAsync<ForbiddenException>(actionAsync);
        }
    }
}
