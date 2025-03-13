using OpenDDD.Infrastructure.Events.Azure.Options;
using OpenDDD.Infrastructure.Events.Kafka.Options;
using OpenDDD.Infrastructure.Events.RabbitMq.Options;

namespace OpenDDD.API.Options
{
    public class OpenDddOptions
    {
        public string PersistenceProvider { get; set; } = "OpenDdd";
        public string DatabaseProvider { get; set; } = "Postgres";
        public string MessagingProvider { get; set; } = "InMemory";

        public OpenDddPostgresOptions Postgres { get; set; } = new();
        public OpenDddSqliteOptions Sqlite { get; set; } = new();
        public OpenDddSqlServerOptions SqlServer { get; set; } = new();
        public OpenDddEventsOptions Events { get; set; } = new();
        public OpenDddAzureServiceBusOptions AzureServiceBus { get; set; } = new();
        public OpenDddRabbitMqOptions RabbitMq { get; set; } = new();
        public OpenDddKafkaOptions Kafka { get; set; } = new();
        public OpenDddAutoRegisterOptions AutoRegister { get; set; } = new();

        // Use Methods for Persistence
        
        public OpenDddOptions UseEfCore()
        {
            PersistenceProvider = "EfCore";
            return this;
        }
        
        public OpenDddOptions UseInMemoryDatabase()
        {
            DatabaseProvider = "InMemory";
            return this;
        }

        public OpenDddOptions UsePostgres(string connectionString)
        {
            DatabaseProvider = "Postgres";
            Postgres.ConnectionString = connectionString;
            return this;
        }

        public OpenDddOptions UseSqlite(string connectionString)
        {
            DatabaseProvider = "SQLite";
            Sqlite.ConnectionString = connectionString;
            return this;
        }
        
        public OpenDddOptions UseSqlServer(string connectionString)
        {
            DatabaseProvider = "SqlServer";
            SqlServer.ConnectionString = connectionString;
            return this;
        }

        // Use Methods for Messaging
        
        public OpenDddOptions UseInMemoryMessaging()
        {
            MessagingProvider = "InMemory";
            return this;
        }

        public OpenDddOptions UseRabbitMq(
            string hostName, 
            int port, 
            string username, 
            string password, 
            string virtualHost = "/", 
            bool autoCreateTopics = true)
        {
            MessagingProvider = "RabbitMq";
            RabbitMq = new OpenDddRabbitMqOptions
            {
                HostName = hostName,
                Port = port,
                Username = username,
                Password = password,
                VirtualHost = virtualHost,
                AutoCreateTopics = autoCreateTopics
            };
            return this;
        }

        public OpenDddOptions UseKafka(string bootstrapServers, bool autoCreateTopics = true)
        {
            MessagingProvider = "Kafka";
            Kafka = new OpenDddKafkaOptions { BootstrapServers = bootstrapServers };
            return this;
        }

        public OpenDddOptions UseAzureServiceBus(string connectionString, bool autoCreateTopics = true)
        {
            MessagingProvider = "AzureServiceBus";
            AzureServiceBus = new OpenDddAzureServiceBusOptions
            {
                ConnectionString = connectionString,
                AutoCreateTopics = autoCreateTopics
            };
            return this;
        }

        // Event Configuration
        public OpenDddOptions SetEventListenerGroup(string group)
        {
            Events.ListenerGroup = group;
            return this;
        }

        public OpenDddOptions SetEventTopics(string domainEventTemplate, string integrationEventTemplate)
        {
            Events.DomainEventTopic = domainEventTemplate;
            Events.IntegrationEventTopic = integrationEventTemplate;
            return this;
        }

        // Auto-Registration Configuration
        public OpenDddOptions EnableAutoRegistration()
        {
            AutoRegister.EnableAll();
            return this;
        }

        public OpenDddOptions DisableAutoRegistration()
        {
            AutoRegister.DisableAll();
            return this;
        }
    }
}
