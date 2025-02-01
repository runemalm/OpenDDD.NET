using OpenDDD.Infrastructure.Events.Azure.Options;

namespace OpenDDD.API.Options
{
    public class OpenDddOptions
    {
        public string PersistenceProvider { get; set; } = "EfCore";
        public string MessagingProvider { get; set; } = "InMemory";
        public OpenDddEfCoreOptions EfCore { get; set; } = new();
        public OpenDddAutoRegisterOptions AutoRegister { get; set; } = new();
        public OpenDddEventsOptions Events { get; set; } = new();
        public OpenDddAzureServiceBusOptions AzureServiceBus { get; set; } = new();

        // Fluent methods for configuring persistence
        public OpenDddOptions UseEfCore()
        {
            PersistenceProvider = "EfCore";
            return this;
        }

        public OpenDddOptions UseSQLite(string connectionString)
        {
            UseEfCore();
            EfCore.Database = "SQLite";
            EfCore.ConnectionString = connectionString;
            return this;
        }

        public OpenDddOptions UsePostgres(string connectionString)
        {
            UseEfCore();
            EfCore.Database = "Postgres";
            EfCore.ConnectionString = connectionString;
            return this;
        }

        public OpenDddOptions UseSqlServer(string connectionString)
        {
            UseEfCore();
            EfCore.Database = "SqlServer";
            EfCore.ConnectionString = connectionString;
            return this;
        }

        // Fluent methods for configuring messaging providers
        public OpenDddOptions UseInMemoryMessaging()
        {
            MessagingProvider = "InMemory";
            return this;
        }

        public OpenDddOptions UseAzureServiceBus(string connectionString, bool autoCreateTopics = true)
        {
            MessagingProvider = "AzureServiceBus";
            AzureServiceBus.ConnectionString = connectionString;
            AzureServiceBus.AutoCreateTopics = autoCreateTopics;
            return this;
        }

        // Fluent methods for event configuration
        public OpenDddOptions SetEventListenerGroup(string group)
        {
            Events.ListenerGroup = group;
            return this;
        }

        public OpenDddOptions SetEventTopicTemplates(string domainEventTopicTemplate, string integrationEventTopicTemplate)
        {
            Events.DomainEventTopicTemplate = domainEventTopicTemplate;
            Events.IntegrationEventTopicTemplate = integrationEventTopicTemplate;
            return this;
        }

        // Fluent methods for auto-registration settings
        public OpenDddOptions EnableAutoRegistration()
        {
            AutoRegister.Actions = true;
            AutoRegister.DomainServices = true;
            AutoRegister.Repositories = true;
            AutoRegister.InfrastructureServices = true;
            AutoRegister.EventListeners = true;
            AutoRegister.EfCoreConfigurations = true;
            return this;
        }

        public OpenDddOptions DisableAutoRegistration()
        {
            AutoRegister.Actions = false;
            AutoRegister.DomainServices = false;
            AutoRegister.Repositories = false;
            AutoRegister.InfrastructureServices = false;
            AutoRegister.EventListeners = false;
            AutoRegister.EfCoreConfigurations = false;
            return this;
        }
    }
}
