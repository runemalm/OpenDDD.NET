using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using Newtonsoft.Json;
using NJsonSchema.Infrastructure;
using dotenv.net;
using WireMock.Server;
using OpenDDD.Application;
using OpenDDD.Application.Error;
using OpenDDD.Application.Settings;
using OpenDDD.Domain.Model.Auth.Exceptions;
using OpenDDD.Domain.Model.BuildingBlocks.Event;
using OpenDDD.Domain.Model.Error;
using OpenDDD.Infrastructure.Ports.Adapters.Common.Translation.Converters;
using OpenDDD.Infrastructure.Ports.Adapters.PubSub.Memory;
using OpenDDD.Infrastructure.Ports.Email;
using OpenDDD.Infrastructure.Ports.PubSub;
using OpenDDD.Infrastructure.Services.Persistence;
using OpenDDD.Logging;

namespace OpenDDD.Tests
{
    public abstract class ActionUnitTests : UnitTests, IAsyncDisposable, IDisposable
    {
        protected string ActionName { get; }
        protected ActionId ActionId { get; }

        public ActionUnitTests()
        {
            ActionName = GetType().Name.Replace("Tests", "");
            ActionId = ActionId.Create();
            UnsetConfigEnvironmentVariables();
        }
        
        public async ValueTask DisposeAsync()
        {
            await PersistenceService.ReleaseConnectionAsync(ActionId);
            TestServer.Dispose();
        }
        
        public void Dispose()
        {
            // We move the blocking TestServer Dispose method outside of the xUnit synchronization context.
            PersistenceService.ReleaseConnection(ActionId);
            Task.Run(() => TestServer.Dispose()).GetAwaiter().GetResult();
        }

        // Configuration
        
        public void UnsetConfigEnvironmentVariables()
        {
            foreach(DictionaryEntry e in Environment.GetEnvironmentVariables())
            {
                if (e.Key.ToString().StartsWith($"CFG_{ActionName}_"))
                {
                    Environment.SetEnvironmentVariable(e.Key.ToString(), null);
                }
            }
        }

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
        
        public IDomainEventAdapter DomainEventAdapter => TestServer.Host.Services.GetRequiredService<IDomainEventAdapter>();
        public IInterchangeEventAdapter InterchangeEventAdapter => TestServer.Host.Services.GetRequiredService<IInterchangeEventAdapter>();
        
        protected void EnableDomainEvents() => DomainPublisher.SetEnabled(true);
        protected void DisableDomainEvents() => DomainPublisher.SetEnabled(false);
        protected void EnableIntegrationEvents() => InterchangePublisher.SetEnabled(true);
        protected void DisableIntegrationEvents() => InterchangePublisher.SetEnabled(false);
        
        protected async Task ReceiveDomainEventAsync(IEvent theEvent)
        {
            var outboxEvent = new OutboxEvent(theEvent, ConversionSettings);
            var message = new MemoryMessage(outboxEvent.JsonPayload);
            var listeners = DomainEventAdapter.GetListeners(
                outboxEvent.EventName,
                outboxEvent.DomainModelVersion.ToStringWithWildcardMinorAndBuildVersions());

            foreach (var listener in listeners)
            {
                var success = await listener.Handle(message);
                if (!success)
                    throw new DddException("Couldn't receive domain event, exception in reaction. See log for reason.");
            }
        }

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
                    // We move the blocking TestServer constructor outside of the xUnit synchronization context.
                    var builder = CreateWebHostBuilder();
                    Task.Run(() => _testServer = new TestServer(builder)).GetAwaiter().GetResult();
                }
                return _testServer;
            }
        }

        protected abstract IWebHostBuilder CreateWebHostBuilder();

        // Translation
        
        public ConversionSettings ConversionSettings => TestServer.Host.Services.GetRequiredService<ConversionSettings>();

        // Persistence
        
        protected IPersistenceService PersistenceService => TestServer.Host.Services.GetRequiredService<IPersistenceService>();
        
        protected void EmptyDb()
        {
            EmptyRepositories();
            EmptyEmailPorts();
            EmptyDeadLetterQueue();
            EmptyOutbox();
        }

        protected async Task EmptyDbAsync()
        {
            await EmptyRepositoriesAsync();
            await EmptyEmailPortsAsync();
            await EmptyDeadLetterQueueAsync();
            await EmptyOutboxAsync();
        }
        
        protected void EmptyRepositories()
        {
            EmptyAggregateRepositories(CancellationToken.None);
        }
        
        protected async Task EmptyRepositoriesAsync()
        {
            await EmptyAggregateRepositoriesAsync(CancellationToken.None);
        }
        
        protected void EmptyEmailPorts()
        {
            EmailAdapter.Empty(CancellationToken.None);
        }
        
        protected async Task EmptyEmailPortsAsync()
        {
            await EmailAdapter.EmptyAsync(CancellationToken.None);
        }
        
        protected void EmptyDeadLetterQueue()
        {
            DeadLetterQueue.Empty(CancellationToken.None);
        }

        protected async Task EmptyDeadLetterQueueAsync()
        {
            await DeadLetterQueue.EmptyAsync(CancellationToken.None);
        }

        protected void EmptyOutbox()
        {
            Outbox.Empty(CancellationToken.None);
        }
        
        protected async Task EmptyOutboxAsync()
        {
            await Outbox.EmptyAsync(CancellationToken.None);
        }
        
        protected abstract void EmptyAggregateRepositories(CancellationToken ct);
        protected abstract Task EmptyAggregateRepositoriesAsync(CancellationToken ct);

        // Email
        
        public IEmailPort EmailAdapter => TestServer.Host.Services.GetRequiredService<IEmailPort>();
        
        protected async Task DoWithEmailDisabled(Func<Task> actionsAsync)
        {
            DisableEmails();
            await actionsAsync();
            EnableEmails();
        }
        
        protected void EnableEmails() => EmailAdapter.SetEnabled(true);
        protected void DisableEmails() => EmailAdapter.SetEnabled(false);

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
                await Assert.ThrowsAsync<AuthorizeException>(actionAsync);
        }

        public async Task AssertFailure<T>(T expected, Task actionTask) where T : DomainException
        {
            var actual = await Assert.ThrowsAsync<T>(async () => await actionTask);
            Assert.Equal(expected, actual);
        }
    }
}
