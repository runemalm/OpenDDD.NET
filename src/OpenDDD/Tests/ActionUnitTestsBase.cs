using System;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using OpenDDD.Application;
using OpenDDD.Application.Error;
using OpenDDD.Domain.Model.Event;
using OpenDDD.Infrastructure.Ports.Adapters.Http;
using OpenDDD.Infrastructure.Services.EventProcessor;
using OpenDDD.Infrastructure.Services.Serialization;
using OpenDDD.Main;
using OpenDDD.NET;
using OpenDDD.NET.Services.DatabaseConnection;
using OpenDDD.NET.Services.Outbox;
using WireMock.Server;
using Xunit;

namespace OpenDDD.Tests
{
    public abstract class ActionUnitTestsBase : UnitTestsBase, IDisposable
    {
        protected string ActionName { get; }
        protected ActionId ActionId { get; }

        public ActionUnitTestsBase()
        {
            ActionName = GetType().Name.Replace("Tests", "");
            ActionId = ActionId.Create();
            
            // Reset date time provider
            DateTimeProvider.Reset();
            
            // Empty database
            ActionDatabaseConnection.Start(CancellationToken.None);
            ActionDatabaseConnection.TruncateDatabase();
            
            // Run the ensure data tasks (that has been wiped, this is kind of a work-around)
            RunEnsureDataTasks();
        }
        
        public void Dispose()
        {
            ActionDatabaseConnection.Stop(CancellationToken.None);

            // We move the blocking TestServer Dispose method outside of the xUnit synchronization context.
            Task.Run(() => TestServer.Dispose()).GetAwaiter().GetResult();
            _scope?.Dispose();
        }

        public void JumpMinutes(int minutes)
        {
            DateTimeProvider.Set(() => DateTime.UtcNow.AddMinutes(minutes));
        }
        
        // Ensure data tasks
        
        protected void RunEnsureDataTasks()
        {
            foreach (var task in Scope.ServiceProvider.GetServices<IEnsureDataTask>())
                task.Execute(ActionId, CancellationToken.None);
        }

        // Mock API
        
        private WireMockServer? _mockApi;
        public WireMockServer MockApi
        {
            get
            {
                if (_mockApi == null)
                    _mockApi = WireMockServer.Start();
                return _mockApi;
            }
        }

        // PubSub
                
        public IDateTimeProvider DateTimeProvider => Scope.ServiceProvider.GetRequiredService<IDateTimeProvider>();

        // PubSub
        
        public IDomainPublisher DomainPublisher => Scope.ServiceProvider.GetRequiredService<IDomainPublisher>();
        // public IInterchangePublisher InterchangePublisher => TestServer.Host.Services.GetRequiredService<IInterchangePublisher>();
        //
        // public IDomainEventAdapter DomainEventAdapter => TestServer.Host.Services.GetRequiredService<IDomainEventAdapter>();
        // public IInterchangeEventAdapter InterchangeEventAdapter => TestServer.Host.Services.GetRequiredService<IInterchangeEventAdapter>();
        
        // protected void EnableDomainEvents() => DomainPublisher.SetEnabled(true);
        // protected void DisableDomainEvents() => DomainPublisher.SetEnabled(false);
        // protected void EnableIntegrationEvents() => InterchangePublisher.SetEnabled(true);
        // protected void DisableIntegrationEvents() => InterchangePublisher.SetEnabled(false);
        
        protected async Task PublishDomainEventAsync(IDomainEvent theEvent)
        {
            await ActionOutbox.AddEventAsync(theEvent);
            await EventProcessor.ProcessNextOutboxEventAsync();
        }

        // Repositories
        
        public IActionOutbox ActionOutbox => Scope.ServiceProvider.GetRequiredService<IActionOutbox>();
        public IEventProcessor EventProcessor => Scope.ServiceProvider.GetRequiredService<IEventProcessor>();

        // Database connections
        
        public IActionDatabaseConnection ActionDatabaseConnection => Scope.ServiceProvider.GetRequiredService<IActionDatabaseConnection>();

        // Logging
        
        public ILogger Logger => TestServer.Host.Services.GetRequiredService<ILogger>();
        
        // Test server
        
        private TestServer? _testServer;
        public TestServer TestServer
        {
            get
            {
                if (_testServer == null)
                {
                    // We move the blocking TestServer constructor outside of the xUnit synchronization context.
                    // See: https://www.strathweb.com/2021/05/the-curious-case-of-asp-net-core-integration-test-deadlock/
                    var builder = CreateWebHostBuilder();
                    Task.Run(() => _testServer = new TestServer(builder)).GetAwaiter().GetResult();
                }
                return _testServer!;
            }
        }
        
        // Test scope
        
        protected IServiceScope? _scope;
        public IServiceScope Scope
        {
            get
            {
                if (_scope == null)
                {
                    _scope = TestServer.Services.CreateScope();
                }
                return _scope;
            }
        }
        
        protected abstract IWebHostBuilder CreateWebHostBuilder();
        
        // Test HTTP Client

        private HttpClient? _testClient;
        public HttpClient TestClient
        {
            get
            {
                if (_testClient == null)
                {
                    _testClient = TestServer.CreateClient(); // Create the client only if it hasn't been created yet
                }
                return _testClient;
            }
        }
        
        // Test HTTP Requests

        protected async Task<HttpResponseMessage> GetAsync(string url)
        {
            var response = await TestClient.GetAsync(url);
            return response;
        }
        
        protected async Task<HttpResponseMessage> PostAsync(string url, RequestBase request)
        {
            var json = JsonConvert.SerializeObject(request, Serializer.Settings.JsonSerializerSettings);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await TestClient.PostAsync(url, content);
            return response;
        }

        // Translation
        
        public ISerializer Serializer => TestServer.Host.Services.GetRequiredService<ISerializer>();

        // Assertions
        
        protected void AssertNow(DateTime? actual)
            => AssertDateWithin200Ms(DateTimeProvider.Now, actual, "The date wasn't equal or close to 'now'.");
        
        public void AssertDomainEventPublished(IDomainEvent theEvent)
            => Assert.True(ActionOutbox.HasPublished(theEvent), $"Expected domain event to have been published: {theEvent.Header.Name}.");

        public void AssertSuccessResponse(HttpResponseMessage response)
        {
            if (!response.IsSuccessStatusCode)
            {
                string content = response.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                AssertTrue(false, $"HTTP request failed ({response.StatusCode}) with error content: '{content}'");
            }
        }

        public async Task AssertFailure<T>(T expected, Task actionTask) where T : DddException
        {
            var actual = await Assert.ThrowsAsync<T>(async () => await actionTask);
            Assert.Equal(expected, actual);
        }
    }
}
