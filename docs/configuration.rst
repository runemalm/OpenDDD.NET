.. _config:

#############
Configuration
#############

OpenDDD.NET provides a flexible configuration system that allows you to customize its behavior in two ways:

1. **appsettings.json** – The settings can be defined in this configuration file.
2. **Fluent Methods** – Configuration can also be done programmatically using fluent methods in the `Program.cs` file.

Both approaches offer full control over the framework's settings, so you can choose the method that best suits your project's needs.

This guide covers the key configuration options available in OpenDDD.NET, including examples for both approaches.

.. _config-general:

################
General Settings
################

The main configuration section for OpenDDD.NET is `"OpenDDD"` in `appsettings.json`.

Example:

.. code-block:: json

    "OpenDDD": {
      "PersistenceProvider": "EfCore",
      "MessagingProvider": "InMemory",
      "AutoRegister": {
        "Actions": true,
        "DomainServices": true,
        "Repositories": true,
        "InfrastructureServices": true,
        "EventListeners": true,
        "EfCoreConfigurations": true
      }
    }

.. _config-general-auto-registration:

-----------------
Auto-Registration
-----------------

OpenDDD.NET automatically registers several components by default.  
You can enable or disable this behavior in the `"AutoRegister"` section.

- **Actions** → Registers all `IAction<TCommand, TResponse>` implementations.
- **Domain Services** → Registers all `IDomainService` implementations.
- **Repositories** → Registers all `IRepository<TAggregate, TId>` implementations.
- **Infrastructure Services** → Registers all `IInfrastructureService` implementations.
- **Event Listeners** → Registers all `EventListenerBase<TEvent, TAction>` implementations.
- **EfCore Configurations** → Registers Entity Framework Core configurations.

To disable auto-registration for a component type, set it to `false`.  

Example:

.. code-block:: json

    "AutoRegister": {
      "Actions": false,
      "DomainServices": true
    }

.. _config-persistence:

#######################
Persistence Settings
#######################

OpenDDD.NET supports **Entity Framework Core (EF Core)** as its persistence provider.

Example configuration:

.. code-block:: json

    "OpenDDD": {
      "PersistenceProvider": "EfCore",
      "EfCore": {
        "Database": "SQLite",
        "ConnectionString": "DataSource=Main/EfCore/Bookstore.db;Cache=Shared"
      }
    }

The **PersistenceProvider** setting determines which persistence provider is used.  
Currently, the only supported provider is `"EfCore"`.

To change database to e.g. **SQL Server**, update the database and connection string:

.. code-block:: json

    "EfCore": {
      "Database": "SqlServer",
      "ConnectionString": "Server=myServer;Database=myDb;User Id=myUser;Password=myPass;"
    }

.. _config-messaging:

##################
Messaging Settings
##################

OpenDDD.NET supports event-driven messaging using **domain events** and **integration events**.  
Each event type has its own **dedicated topic**.

Example configuration:

.. code-block:: json

    "OpenDDD": {
      "MessagingProvider": "AzureServiceBus",
      "Events": {
        "DomainEventTopicTemplate": "Bookstore.Domain.{EventName}",
        "IntegrationEventTopicTemplate": "Bookstore.Interchange.{EventName}",
        "ListenerGroup": "Default"
      },
      "AzureServiceBus": {
        "ConnectionString": "Endpoint=sb://your-servicebus.servicebus.windows.net/;SharedAccessKeyName=your-key;SharedAccessKey=your-key",
        "AutoCreateTopics": true
      }
    }

------------------
Messaging Provider
------------------

**MessagingProvider** specifies the message bus to be used for event processing:

- `"InMemory"` → Local message bus for event processing within the same application instance.
- `"AzureServiceBus"` → Distributed message bus for event processing across services.

------------------------
Topic Naming Conventions
------------------------

- **Domain Events:** `"Bookstore.Domain.{EventName}"`  
  (or `"Bookstore.{BoundedContext}.{EventName}"` for multi-context applications)
- **Integration Events:** `"Bookstore.Interchange.{EventName}"`  
  (Always includes `"Interchange"` as the middle part)

----------------------
Competing Consumer Pattern
----------------------

OpenDDD.NET supports the **competing consumer pattern**, allowing multiple instances of a service  
to process messages from the same event topic. 

**ListenerGroup** specifies which **consumer group** the application instance belongs to.  
Services in the same listener group compete for messages, ensuring **load balancing**  
while preventing duplicate processing within the group.

.. _config-fluent:

############################
Fluent Configuration Example
############################

OpenDDD.NET can also be configured using fluent methods in `Program.cs` when adding the services.

Here’s an example configuration using the fluent API:

.. code-block:: csharp

    builder.Services.AddOpenDDD<BookstoreDbContext>(builder.Configuration, 
        options =>  
        {  
            options.UseEfCore()
                   .UseSQLite("DataSource=Main/EfCore/Bookstore.db;Cache=Shared")
                   .UseInMemoryMessaging()
                   .SetEventListenerGroup("Default")
                   .SetEventTopicTemplates(
                       "Bookstore.Domain.{EventName}",
                       "Bookstore.Interchange.{EventName}"
                    )
                   .EnableAutoRegistration();
        }
    );

This method allows you to customize the framework’s settings programmatically without needing to rely on the `appsettings.json` file.
