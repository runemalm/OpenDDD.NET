.. _config:

#############
Configuration
#############

OpenDDD.NET **leverages .NET's built-in configuration system**, allowing you to define settings using standard .NET conventions.

You can configure OpenDDD.NET using:

1. **appsettings.json** – Define structured configuration in a file.
2. **Fluent API** – Configure programmatically in `Program.cs`.

These options allow flexibility based on your **environment**, **deployment strategy**, and **runtime needs**.

This guide covers **persistence, messaging, auto-registration, and fluent configuration**, with examples for both approaches.

*******************
1. General Settings
*******************

The **main configuration section** for OpenDDD.NET is `"OpenDDD"` in `appsettings.json`:

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

**Key settings:**

- **PersistenceProvider** → Defines the database provider (e.g., `"EfCore"`).
- **MessagingProvider** → Sets the event messaging system (`"InMemory"` or `"AzureServiceBus"`).
- **AutoRegister** → Controls automatic registration of components.

.. _config-auto-registration:

********************
2. Auto-Registration
********************

By default, OpenDDD.NET **automatically registers** common components:

- **Actions** → `IAction<TCommand, TResponse>`
- **Domain Services** → `IDomainService`
- **Repositories** → `IRepository<TAggregate, TId>`
- **Infrastructure Services** → `IInfrastructureService`
- **Event Listeners** → `EventListenerBase<TEvent, TAction>`
- **EF Core Configurations** → `EfAggregateRootConfigurationBase<TAggregateRoot, TId>` and `EfEntityConfigurationBase<TEntity, TId>`

To disable auto-registration for specific components, set them to `false`:

.. code-block:: json

    "AutoRegister": {
      "Actions": false,
      "DomainServices": true
    }

**************
3. Persistence
**************

OpenDDD.NET **uses EF Core** for persistence. Configure it in `appsettings.json`:

.. code-block:: json

    "OpenDDD": {
      "PersistenceProvider": "EfCore",
      "EfCore": {
        "Database": "SQLite",
        "ConnectionString": "DataSource=Infrastructure/Persistence/EfCore/Bookstore.db;Cache=Shared"
      }
    }

To switch to **SQL Server**, update the `Database` and `ConnectionString`:

.. code-block:: json

    "EfCore": {
      "Database": "SqlServer",
      "ConnectionString": "Server=myServer;Database=myDb;User Id=myUser;Password=myPass;"
    }

You can choose any database provider `supported by EF Core <https://learn.microsoft.com/en-us/ef/core/providers/?tabs=dotnet-core-cli>`_.

************
4. Messaging
************

OpenDDD.NET **supports event-driven architecture** using **domain events** and **integration events**.

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

-----------------------
**Messaging Providers**
-----------------------

- `"InMemory"` → Local event processing.
- `"AzureServiceBus"` → Distributed event processing across services.

We will add more messaging providers as we go. If you want to create a provider, you can check out `how to contribute to the source code <https://github.com/runemalm/OpenDDD.NET/blob/master/CONTRIBUTING.md>`_.

----------------------------
**Topic Naming Conventions**
----------------------------

- **Domain Events:** `"Bookstore.Domain.{EventName}"`
- **Integration Events:** `"Bookstore.Interchange.{EventName}"`

Use `Domain` when you have a single bounded context. Replace it with the specific name when you have multiple, (e.g. Bookstore.Booking.{EventName}).

Since there will only ever be one interchange context, the `Ìnterchange` part will never change for integration event topics.

------------------------------
**Listener Groups & Scaling**
------------------------------

OpenDDD.NET **supports horizontal scaling** by allowing multiple service instances to process events concurrently.  

This is achieved using **Listener Groups**:  

- **All instances within the same Listener Group** compete for messages, ensuring events are processed only once.  
- **Each deployed instance of a service** can scale horizontally while preventing duplicate processing.  

**Example: Scaling a Monolith**

If you deploy a **Bookstore monolith** with multiple instances, all instances can share the same listener group:

.. code-block:: json

    "Events": {
      "ListenerGroup": "Default"
    }

**Example: Scaling Multiple Services**

If your system has **multiple services**, each service group can use its own Listener Group:

.. code-block:: json

    "Events": {
      "ListenerGroup": "Booking"
    }

**Result:**  

- All instances of the **Booking** service compete for events in the `"Booking"` group.
- The **Shipping** service can have its own `"Service"` listener group.
- The **Catalogue** service can have its own `"Catalogue"` listener group, etc..
- This enables **independent scaling** for each group of services.

Read more about the `Competing Consumer <https://learn.microsoft.com/en-us/azure/architecture/patterns/competing-consumers>`_ pattern here.

***********************
5. Fluent Configuration
***********************

Instead of `appsettings.json`, OpenDDD.NET can be configured **dynamically** in `Program.cs`:

.. code-block:: csharp

    builder.Services.AddOpenDDD<BookstoreDbContext>(builder.Configuration, 
        options =>  
        {  
            options.UseEfCore()
                   .UseSQLite("DataSource=Infrastructure/Persistence/EfCore/Bookstore.db;Cache=Shared")
                   .UseInMemoryMessaging()
                   .SetEventListenerGroup("Default")
                   .SetEventTopicTemplates(
                       "Bookstore.Domain.{EventName}",
                       "Bookstore.Interchange.{EventName}"
                    )
                   .EnableAutoRegistration();
        }
    );

**Advantages of Fluent Configuration:**

- **Dynamic setup** without modifying `appsettings.json`.
- **Overrides JSON settings** when used together.
- **Useful for runtime configuration and dynamic overrides**.

*******
Summary
*******

- **Choose JSON and/or Fluent API** → Both fully configure OpenDDD.NET.  
- **Define persistence, messaging, and auto-registration** based on your needs.  
- **Use listener groups** to scale applications horizontally.  
- **Follow event naming conventions** for structured messaging.  
