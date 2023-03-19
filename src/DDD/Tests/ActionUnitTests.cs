using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using WireMock.Server;
using dotenv.net;
using Newtonsoft.Json;
using NJsonSchema.Infrastructure;
using DDD.Application;
using DDD.Application.Settings;
using DDD.Domain.Model.Auth.Exceptions;
using DDD.Domain.Model.BuildingBlocks.Event;
using DDD.Domain.Model.Error;
using DDD.Infrastructure.Ports.Adapters.Common.Translation.Converters;
using DDD.Infrastructure.Ports.Email;
using DDD.Infrastructure.Ports.PubSub;
using DDD.Infrastructure.Services.Persistence;
using DDD.Logging;

namespace DDD.Tests
{
    public abstract class ActionUnitTests : UnitTests, IAsyncDisposable, IDisposable
    {
        protected string ActionName { get; }
        protected ActionId ActionId { get; }
        
        public ActionUnitTests()
        {
            ActionName = GetType().Name.Replace("Tests", "");
            ActionId = ActionId.Create();
            
            Debug.WriteLine($"{DateTime.UtcNow:HH:mm:ss.fff} [{ActionName}] BaseConstructor()");
        }

        public async ValueTask DisposeAsync()
        {
            Debug.WriteLine($"{DateTime.UtcNow:HH:mm:ss.fff} [{ActionName}] BaseDisposeAsync()");
            await PersistenceService.ReleaseConnectionAsync(ActionId);
            await TestServer.Host.StopAsync();
        }
        
        public void Dispose()
        {
            PersistenceService.ReleaseConnectionAsync(ActionId).GetAwaiter().GetResult();
            TestServer.Host.StopAsync().GetAwaiter().GetResult();
        }

        // Configuration
        
        public void Configure()
            => Environment.SetEnvironmentVariable($"ENV_FILE_{ActionName}", CreateEnvFileJson());

        private string CreateEnvFileJson()
        {
            var envFileName = "env.test";
            
            var opts = 
                DotEnv.Fluent()
                    .WithExceptions()
                    .WithEnvFiles(GetEnvFilePath(envFileName))
                    .WithTrimValues();

            var values = opts.Read()
                .Select(kvp => new KeyValuePair<string, string>(Regex.Replace(kvp.Key, "CFG_", $"CFG_{ActionName}_"), kvp.Value))
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value);

            var existing = 
                Environment.GetEnvironmentVariables()
                    .Cast<DictionaryEntry>()
                    .Where(kvp => kvp.Key.ToString().StartsWith($"CFG_{ActionName}_"))
                    .Select(entry => new KeyValuePair<string, string>(entry.Key.ToString(), entry.Value.ToString()));

            values[$"CFG_{ActionName}_POSTGRES_CONN_STR"] = Regex.Replace(
                values[$"CFG_{ActionName}_POSTGRES_CONN_STR"], 
                "Database=([^;]*)", 
                $"Database=$1_{ActionName}");

            foreach (var e in existing)
                values[e.Key] = e.Value;

            var jsonResolver = new PropertyRenameAndIgnoreSerializerContractResolver();
            
            var serializerSettings = new JsonSerializerSettings();
            serializerSettings.ContractResolver = jsonResolver;
            
            var jsonString = JsonConvert.SerializeObject(values, serializerSettings);

            return jsonString;
        }
        
        private string GetEnvFilePath(string filename)
        {
            var pathRoot = Path.GetPathRoot(Directory.GetCurrentDirectory());

            var dir = Directory.GetCurrentDirectory();
            var path = $"{dir}/{filename}";
            bool found = File.Exists(path);

            while (!found && dir != pathRoot)
            {
                dir = Directory.GetParent(dir).ToString();
                path = dir != "/" ? $"{dir}/{filename}" : $"/{filename}";
                found = File.Exists(path);
            }

            return path;
        }

        public void SetConfig(string name, string value)
            => Environment.SetEnvironmentVariable($"CFG_{ActionName}_{name}", value);
        
        public string? GetConfig(string name)
            => Environment.GetEnvironmentVariable($"CFG_{ActionName}_{name}");

        public void SetFrontendConfig(string name, string value)
            => SetConfig($"FRONTEND_{name}", value);

        public void SetConfigPersistenceProvider(string value)
            => SetConfig("PERSISTENCE_PROVIDER", value);
        
        public void SetConfigPostgresConnStr(string value)
            => SetConfig("POSTGRES_CONN_STR", value);

        public string? GetConfigPostgresConnStr()
            => GetConfig("POSTGRES_CONN_STR");

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
        
        protected void EnableDomainEvents() => DomainPublisher.SetEnabled(true);
        protected void DisableDomainEvents() => DomainPublisher.SetEnabled(false);
        protected void EnableIntegrationEvents() => InterchangePublisher.SetEnabled(true);
        protected void DisableIntegrationEvents() => InterchangePublisher.SetEnabled(false);

        // Repositories
        
        public IOutbox Outbox => TestServer.Host.Services.GetRequiredService<IOutbox>();
        public IDeadLetterQueue DeadLetterQueue => TestServer.Host.Services.GetRequiredService<IDeadLetterQueue>();
        
        // Settings
        
        public ISettings Settings => TestServer.Host.Services.GetRequiredService<ISettings>();
        
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
                    var builder = CreateWebHostBuilder();
                    _testServer = new TestServer(builder);
                }
                return _testServer;
            }
        }

        protected abstract IWebHostBuilder CreateWebHostBuilder();

        // Translation
        
        public SerializerSettings SerializerSettings => TestServer.Host.Services.GetRequiredService<SerializerSettings>();

        // Persistence
        
        protected IPersistenceService PersistenceService => TestServer.Host.Services.GetRequiredService<IPersistenceService>();
        
        protected async Task EmptyDb()
        {
            await EmptyRepositories();
            await EmptyEmailPorts();
            await EmptyDeadLetterQueue();
            await EmptyOutbox();
        }
        
        protected async Task EmptyRepositories()
        {
            await EmptyAggregateRepositories(CancellationToken.None);
        }
        
        protected async Task EmptyEmailPorts()
        {
            await EmailAdapter.EmptyAsync(CancellationToken.None);
        }
        
        protected async Task EmptyDeadLetterQueue()
        {
            await DeadLetterQueue.EmptyAsync(CancellationToken.None);
        }

        protected async Task EmptyOutbox()
        {
            await Outbox.EmptyAsync(CancellationToken.None);
        }
        
        protected abstract Task EmptyAggregateRepositories(CancellationToken ct);

        // Email
        
        public IEmailPort EmailAdapter => TestServer.Host.Services.GetRequiredService<IEmailPort>();

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
                await Assert.ThrowsAsync<ForbiddenException>(actionAsync);
        }

        public async Task AssertFailure<T>(T expected, Task actionTask) where T : DomainException
        {
            var actual = await Assert.ThrowsAsync<T>(async () => await actionTask);
            Assert.Equal(expected, actual);
        }
    }
}
