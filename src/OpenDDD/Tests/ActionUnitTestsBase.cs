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
            
            // UnsetConfigEnvironmentVariables();
            
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

        // Configuration
        
        // public void UnsetConfigEnvironmentVariables()
        // {
        //     foreach(DictionaryEntry e in Environment.GetEnvironmentVariables())
        //     {
        //         if (e.Key.ToString().StartsWith($"CFG_{ActionName}_"))
        //         {
        //             Environment.SetEnvironmentVariable(e.Key.ToString(), null);
        //         }
        //     }
        // }
        //
        // public void Configure()
        //     => Environment.SetEnvironmentVariable($"ENV_FILE_{ActionName}", CreateEnvFileJson());
        //
        // private string CreateEnvFileJson()
        // {
        //     var envFileName = "env.test";
        //     
        //     var opts = 
        //         DotEnv.Fluent()
        //             .WithExceptions()
        //             .WithEnvFiles(GetEnvFilePath(envFileName))
        //             .WithTrimValues();
        //
        //     var values = opts.Read()
        //         .Select(kvp => new KeyValuePair<string, string>(Regex.Replace(kvp.Key, "CFG_", $"CFG_{ActionName}_"), kvp.Value))
        //         .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
        //
        //     var existing = 
        //         Environment.GetEnvironmentVariables()
        //             .Cast<DictionaryEntry>()
        //             .Where(kvp => kvp.Key.ToString().StartsWith($"CFG_{ActionName}_"))
        //             .Select(entry => new KeyValuePair<string, string>(entry.Key.ToString(), entry.Value.ToString()));
        //
        //     values[$"CFG_{ActionName}_POSTGRES_CONN_STR"] = Regex.Replace(
        //         values[$"CFG_{ActionName}_POSTGRES_CONN_STR"], 
        //         "Database=([^;]*)", 
        //         $"Database=$1_{ActionName}");
        //
        //     foreach (var e in existing)
        //         values[e.Key] = e.Value;
        //
        //     var jsonResolver = new PropertyRenameAndIgnoreSerializerContractResolver();
        //     
        //     var serializerSettings = new JsonSerializerSettings();
        //     serializerSettings.ContractResolver = jsonResolver;
        //     
        //     var jsonString = JsonConvert.SerializeObject(values, serializerSettings);
        //
        //     return jsonString;
        // }
        //
        // private string GetEnvFilePath(string filename)
        // {
        //     var pathRoot = Path.GetPathRoot(Directory.GetCurrentDirectory());
        //
        //     var dir = Directory.GetCurrentDirectory();
        //     var path = $"{dir}/{filename}";
        //     bool found = File.Exists(path);
        //
        //     while (!found && dir != pathRoot)
        //     {
        //         dir = Directory.GetParent(dir).ToString();
        //         path = dir != "/" ? $"{dir}/{filename}" : $"/{filename}";
        //         found = File.Exists(path);
        //     }
        //
        //     return path;
        // }
        //
        // public void SetConfig(string name, string value)
        //     => Environment.SetEnvironmentVariable($"CFG_{ActionName}_{name}", value);
        //
        // public string? GetConfig(string name)
        //     => Environment.GetEnvironmentVariable($"CFG_{ActionName}_{name}");
        //
        // public void SetFrontendConfig(string name, string value)
        //     => SetConfig($"FRONTEND_{name}", value);
        //
        // public void SetConfigPersistenceProvider(string value)
        //     => SetConfig("PERSISTENCE_PROVIDER", value);
        //
        // public void SetConfigPostgresConnStr(string value)
        //     => SetConfig("POSTGRES_CONN_STR", value);
        //
        // public string? GetConfigPostgresConnStr()
        //     => GetConfig("POSTGRES_CONN_STR");

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
        
        // protected async Task ReceiveDomainEventAsync(IEvent theEvent)
        // {
        //     var outboxEvent = OutboxEvent.Create(theEvent, ConversionSettings, DateTimeProvider);
        //     var message = new MemoryMessage(outboxEvent.JsonPayload);
        //     var listeners = DomainEventAdapter.GetListeners(
        //         outboxEvent.EventName,
        //         outboxEvent.DomainModelVersion.ToStringWithWildcardMinorAndBuildVersions());
        //
        //     foreach (var listener in listeners)
        //     {
        //         var success = await listener.Handle(message);
        //         if (!success)
        //             throw new DddException(
        //                 "An exception occured when reacting to the domain event. See the debug log for details.");
        //     }
        // }

        // Repositories
        
        public IActionOutbox ActionOutbox => Scope.ServiceProvider.GetRequiredService<IActionOutbox>();
        // public IDeadLetterQueue DeadLetterQueue => TestServer.Host.Services.GetRequiredService<IDeadLetterQueue>();
        
        // Database connections
        
        public IActionDatabaseConnection ActionDatabaseConnection => Scope.ServiceProvider.GetRequiredService<IActionDatabaseConnection>();

        // Logging
        
        public ILogger Logger => TestServer.Host.Services.GetRequiredService<ILogger>();
        
        // Test server
        
        private TestServer _testServer;
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
                return _testServer;
            }
        }
        
        // Test scope
        
        protected IServiceScope _scope;
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

        private HttpClient _testClient;
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
        
        // Email
        
        // public IEmailPort EmailAdapter => TestServer.Host.Services.GetRequiredService<IEmailPort>();
        //
        // protected async Task DoWithEmailDisabled(Func<Task> actionsAsync)
        // {
        //     DisableEmails();
        //     await actionsAsync();
        //     EnableEmails();
        // }
        
        // protected async Task DoWithEventsDisabled(Func<Task> actionsAsync)
        // {
        //     var prevDomainEventsEnabled = DomainPublisher.IsEnabled();
        //     var prevIntegrationEventsEnabled = InterchangePublisher.IsEnabled();
        //     
        //     DisableDomainEvents();
        //     DisableIntegrationEvents();
        //     await actionsAsync();
        //     
        //     if (prevDomainEventsEnabled)
        //         EnableDomainEvents();
        //     
        //     if (prevIntegrationEventsEnabled)
        //         EnableIntegrationEvents();
        // }
        //
        // protected void EnableEmails() => EmailAdapter.SetEnabled(true);
        // protected void DisableEmails() => EmailAdapter.SetEnabled(false);
        
        // Assertions
        
        protected void AssertNow(DateTime? actual)
            => AssertDateWithin200Ms(DateTimeProvider.Now, actual, "The date wasn't equal or close to 'now'.");
        
        public void AssertDomainEventPublished(IDomainEvent theEvent)
            => Assert.True(ActionOutbox.HasPublished(theEvent), $"Expected domain event to have been published: {theEvent.Header.Name}.");
        
        // public void AssertIntegrationEventPublished(Event event_)
        //     => Assert.True(InterchangePublisher.HasPublished(event_), $"Expected integration event to have been published: {event_.Header.Name}.");
        //
        // public async Task AssertAllowedAsync<T>(bool shouldBeAllowed, Func<Task<T>> actionAsync)
        // {
        //     if (shouldBeAllowed)
        //         await actionAsync();
        //     else
        //         await Assert.ThrowsAsync<AuthorizeException>(actionAsync);
        // }
        //
        // public async Task AssertCommandValidationFailure(ICommand command, IEnumerable<(string, string)> expectedErrors)
        // {
        //     var exc = Assert.Throws<ApplicationException>(command.Validate);
        //     AssertCount(1, exc.Errors);
        //     AssertEqual(ApplicationError.Application_InvalidCommand_Code, exc.Errors.Single().Code);
        //     AssertEqual(
        //         $"Invalid command: {string.Join(". ", expectedErrors.Select(e => $"Field: '{command.GetType().Name}.{e.Item1}', Message: '{e.Item2}'"))}", 
        //         exc.Errors.Single().Message);
        // }

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
