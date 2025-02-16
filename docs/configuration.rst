.. _config:

===================
Configuration Guide
===================

OpenDDD.NET provides a flexible configuration system to set up persistence, messaging, event handling, and auto-registration. 

Configuration is typically done via `appsettings.json` or by using fluent methods in `OpenDddOptions`.

.. contents:: Table of Contents
   :local:
   :depth: 2

------------------
JSON Configuration
------------------

An example configuration in `appsettings.json`:

.. code-block:: json

   {
     "OpenDDD": {
       "PersistenceProvider": "OpenDdd",
       "DatabaseProvider": "InMemory",
       "MessagingProvider": "InMemory",
       "Events": {
         "DomainEventTopicTemplate": "Bookstore.Domain.{EventName}",
         "IntegrationEventTopicTemplate": "Bookstore.Interchange.{EventName}",
         "ListenerGroup": "Default"
       },
       "SQLite": {
         "ConnectionString": "DataSource=Infrastructure/Persistence/EfCore/Bookstore.db;Cache=Shared"
       },
       "Postgres": {
         "ConnectionString": "Host=localhost;Port=5432;Database=bookstore;Username=postgres;Password=password"
       },
       "AzureServiceBus": {
         "ConnectionString": "Endpoint=sb://your-namespace.servicebus.windows.net/;SharedAccessKeyName=your-key-name;SharedAccessKey=your-key",
         "AutoCreateTopics": true
       },
       "RabbitMq": {
         "HostName": "localhost",
         "Port": 5672,
         "Username": "guest",
         "Password": "guest",
         "VirtualHost": "/"
       },
       "Kafka": {
         "BootstrapServers": "localhost:9092"
       },
       "AutoRegister": {
         "Actions": true,
         "DomainServices": true,
         "Repositories": true,
         "InfrastructureServices": true,
         "EventListeners": true,
         "EfCoreConfigurations": true,
         "Seeders": true
       }
     }
   }

--------------------
Fluent Configuration
--------------------

Instead of using `appsettings.json`, OpenDDD.NET can be configured **dynamically** in `Program.cs`:

.. code-block:: csharp

    builder.Services.AddOpenDDD(builder.Configuration, 
        options =>  
        {  
            options.UseInMemoryDatabase()
                   .UseInMemoryMessaging()
                   .SetEventTopicTemplates(
                       "Bookstore.Domain.{EventName}",
                       "Bookstore.Interchange.{EventName}"
                    )
                   .SetEventListenerGroup("Default")
                   .EnableAutoRegistration();
        }
    );

-------------------------
Persistence Configuration
-------------------------

OpenDDD.NET supports multiple persistence providers. Each persistence provider supports a set of database providers.

Example Configurations:

**OpenDDD Persistence Provider**:

.. code-block:: csharp

   // With PostgreSQL
   options.UsePostgres("Host=localhost;Port=5432;Database=bookstore;Username=postgres;Password=password");

   // With In-Memory
   options.UseInMemory();

**EF Core Persistence Provider**:

.. code-block:: csharp

   // With SQLite
   options.UseEfCore().UseSQLite("DataSource=Bookstore.db;Cache=Shared");

   // With PostgreSQL
   options.UseEfCore().UsePostgres("Host=localhost;Port=5432;Database=bookstore;Username=postgres;Password=password");

   // With SQL Server
   options.UseEfCore().UseSqlServer("Server=localhost;Database=bookstore;User Id=sa;Password=password;");

-----------------------
Messaging Configuration
-----------------------

OpenDDD.NET supports multiple messaging providers:

**In-Memory Messaging**:

.. code-block:: csharp

   options.UseInMemoryMessaging();

**RabbitMQ**:

.. code-block:: csharp

   options.UseRabbitMq(
       hostName: "localhost",
       port: 5672,
       username: "guest",
       password: "guest",
       virtualHost: "/"
   );

**Kafka**:

.. code-block:: csharp

   options.UseKafka("localhost:9092");

**Azure Service Bus**:

.. code-block:: csharp

   options.UseAzureServiceBus(
       "Endpoint=sb://your-namespace.servicebus.windows.net/;SharedAccessKeyName=your-key-name;SharedAccessKey=your-key",
       autoCreateTopics: true
   );

.. _config-events:

-------------------
Event Configuration
-------------------

Event settings define how domain and integration events are published:

.. code-block:: csharp

   options.SetEventTopicTemplates(
             "Bookstore.Domain.{EventName}", 
             "Bookstore.Interchange.{EventName}"
          )
          .SetEventListenerGroup("Default");

.. _config-auto-registration:

-----------------
Auto-Registration
-----------------

OpenDDD.NET can automatically register key components:

.. code-block:: csharp

   options.EnableAutoRegistration();

To disable auto-registration:

.. code-block:: csharp

   options.DisableAutoRegistration();

You can also configure individual registrations:

.. code-block:: json

   {
     "OpenDDD": {
       "AutoRegister": {
         "Actions": true,
         "DomainServices": true,
         "Repositories": true,
         "InfrastructureServices": true,
         "EventListeners": true,
         "EfCoreConfigurations": true,
         "Seeders": true
       }
     }
   }

----------
Next Steps
----------

- See :ref:`Getting Started <userguide-getting-started>` for setting up a new project.
- See a full implementation in the `Bookstore Sample Project <https://github.com/runemalm/OpenDDD.NET/tree/master/samples/Bookstore>`_ on GitHub.  
- Go to the :ref:`Building Blocks <building-blocks>` section, for full documentation on each DDD building block.
- Get involved in the `OpenDDD.NET Discussions <https://github.com/runemalm/OpenDDD.NET/discussions>`_ to ask questions, share insights, and contribute.  
