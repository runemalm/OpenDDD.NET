using OpenDDD.Infrastructure.Events.Azure.Options;
using OpenDDD.Infrastructure.Events.Kafka.Options;
using OpenDDD.Infrastructure.Events.RabbitMq.Options;

namespace OpenDDD.API.Options
{
    public class OpenDddOptions
    {
        public string PersistenceProvider { get; set; } = "EfCore";
        public string MessagingProvider { get; set; } = "InMemory";
        public OpenDddEfCoreOptions EfCore { get; set; } = new();
        public OpenDddPersistenceOptions OpenDddPersistenceProvider { get; set; } = new();
        public OpenDddAutoRegisterOptions AutoRegister { get; set; } = new();
        public OpenDddEventsOptions Events { get; set; } = new();
        public OpenDddAzureServiceBusOptions AzureServiceBus { get; set; } = new();
        public OpenDddRabbitMqOptions RabbitMq { get; set; } = new();
        public OpenDddKafkaOptions Kafka { get; set; } = new();

        // Fluent methods for configuring persistence
        public OpenDddOptions UseOpenDddPersistence()
        {
            PersistenceProvider = "OpenDDD";
            return this;
        }

        public OpenDddOptions UseOpenDddPostgres(string connectionString)
        {
            UseOpenDddPersistence();
            OpenDddPersistenceProvider.Database = "Postgres";
            OpenDddPersistenceProvider.ConnectionString = connectionString;
            return this;
        }

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
        
        public OpenDddOptions UseRabbitMq(
            string? hostName = null, 
            int? port = null, 
            string? username = null, 
            string? password = null, 
            string? virtualHost = null)
        {
            MessagingProvider = "RabbitMq";

            RabbitMq.HostName ??= hostName;
            if (port.HasValue) 
            {
                RabbitMq.Port = port.Value;
            }
            RabbitMq.Username ??= username;
            RabbitMq.Password ??= password;
            RabbitMq.VirtualHost ??= virtualHost;

            return this;
        }

        public OpenDddOptions UseKafka(string? bootstrapServers = null)
        {
            MessagingProvider = "Kafka";
            Kafka.BootstrapServers ??= bootstrapServers;
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
            AutoRegister.Seeders = true;
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
            AutoRegister.Seeders = false;
            return this;
        }
    }
}
