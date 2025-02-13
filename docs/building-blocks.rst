.. note::

    OpenDDD.NET is currently in alpha. Features and documentation are under active development and subject to change.

.. _building-blocks:

########
Overview
########

OpenDDD.NET provides implementations of the tactical design **building blocks** used in **Domain-Driven Design (DDD)**.  
Each block serves a specific purpose in organizing business logic, enforcing boundaries, and maintaining consistency.

.. _building-blocks-aggregates:

##########
Aggregates
##########

An **aggregate** is a cluster of domain objects that are treated as a single unit. OpenDDD.NET provides implementations for **Aggregate Roots, Entities, and Value Objects** to maintain consistency and encapsulation.

Aggregate Root
--------------

An **Aggregate Root** is the entry point to an aggregate. It enforces invariants and ensures that all modifications go through it.

.. code-block:: csharp

    using OpenDDD.Domain.Model.Base;

    namespace Bookstore.Domain.Model
    {
        public class Order : AggregateRootBase<Guid>
        {
            public Guid CustomerId { get; private set; }
            public ICollection<LineItem> LineItems { get; private set; }

            private Order(Guid id, Guid customerId) : base(id)
            {
                CustomerId = customerId;
                LineItems = new List<LineItem>();
            }

            public static Order Create(Guid customerId)
            {
                return new Order(Guid.NewGuid(), customerId);
            }

            public void AddLineItem(Guid bookId, float price)
            {
                var lineItem = LineItem.Create(bookId, price);
                LineItems.Add(lineItem);
            }
        }
    }

Entity
------

An **Entity** has a unique identity within an aggregate and a lifecycle managed by its Aggregate Root.

.. code-block:: csharp

    using OpenDDD.Domain.Model.Base;

    namespace Bookstore.Domain.Model
    {
        public class LineItem : EntityBase<Guid>
        {
            public Guid BookId { get; private set; }
            public float Price { get; private set; }

            private LineItem(Guid id, Guid bookId, float price) : base(id)
            {
                BookId = bookId;
                Price = price;
            }

            public static LineItem Create(Guid bookId, float price)
            {
                return new LineItem(Guid.NewGuid(), bookId, price);
            }
        }
    }

Value Object
------------

A **Value Object** represents a concept with no unique identity. They are immutable and define attributes.

.. code-block:: csharp

    using OpenDDD.Domain.Model.Base;

    namespace Bookstore.Domain.Model
    {
        public class Money : IValueObject
        {
            public decimal Amount { get; }
            public string Currency { get; }

            public Money(decimal amount, string currency)
            {
                Amount = amount;
                Currency = currency;
            }
        }
    }

.. _building-blocks-repositories:

############
Repositories
############

Repositories provide a **collection-like interface** for retrieving and persisting aggregates. All repositories implement `IRepository<TAggregateRoot, TId>`, ensuring a **consistent API** and **clear naming conventions**. Aggregates are stored as **serialized JSON documents** in the configured database.

IRepository<TAggregateRoot, TId>
--------------------------------

All repositories implement `IRepository<TAggregateRoot, TId>`, which provides standard data access methods:

.. code-block:: csharp

    using System.Linq.Expressions;

    namespace OpenDDD.Domain.Model
    {
        public interface IRepository<TAggregateRoot, in TId> 
            where TAggregateRoot : IAggregateRoot<TId>
            where TId : notnull
        {
            Task<TAggregateRoot> GetAsync(TId id, CancellationToken ct);
            Task<TAggregateRoot?> FindAsync(TId id, CancellationToken ct);
            Task<IEnumerable<TAggregateRoot>> FindWithAsync(Expression<Func<TAggregateRoot, bool>> filterExpression, CancellationToken ct);
            Task<IEnumerable<TAggregateRoot>> FindAllAsync(CancellationToken ct);
            Task SaveAsync(TAggregateRoot aggregateRoot, CancellationToken ct);
            Task DeleteAsync(TAggregateRoot aggregateRoot, CancellationToken ct);
        }
    }

Method Naming Conventions
-------------------------

All repositories follow a **consistent naming convention** for data retrieval:

.. list-table::
   :header-rows: 1

   * - **Method**
     - **Description**
     - **Behavior**
   * - **GetAsync**
     - Retrieves a single aggregate by Id
     - **Throws** if not found
   * - **FindAsync**
     - Retrieves a single aggregate by Id
     - Returns `null` if not found
   * - **FindBy**
     - Retrieves a single aggregate by a specific field
     - **Throws** if multiple exist
   * - **FindWithAsync**
     - Retrieves multiple aggregates matching a filter
     - Returns a **collection**
   * - **FindAllAsync**
     - Retrieves all aggregates of a type
     - Returns a **collection**
   * - **SaveAsync**
     - Saves an aggregate
     - Inserts if new, updates if existing
   * - **DeleteAsync**
     - Deletes an aggregate
     - Removes it from the repository

.. note::  

    The terms **Get**, **Find**, **By**, and **With** have specific semantics in method names.

Auto-Registration
-----------------

Repositories are **auto-registered** with `IRepository<TAggregateRoot, TId>`. If a custom repository interface exists (e.g., `ICustomerRepository`), it is registered with its corresponding implementation instead.

**Example: Default Auto-Registered Repository**

- `IRepository<Guid, Customer>` → `PostgresOpenDddRepository<Guid, Customer>`

**Example: Custom Auto-Registered Repository**

- `ICustomerRepository` → `PostgresOpenDddCustomerRepository`

Auto-registration can be **disabled in the configuration**.

Create a Custom Repository
--------------------------

If an aggregate requires additional query methods, create a **custom repository** by subclassing a base repository class for your configured database- and persistence provider.

**Example: Custom PostgreSQL Repository**

.. code-block:: csharp

    using OpenDDD.Infrastructure.Persistence.OpenDdd.DatabaseSession.Postgres;
    using OpenDDD.Infrastructure.Repository.OpenDdd.Postgres;
    using OpenDDD.Infrastructure.Persistence.Serializers;
    using Npgsql;
    using Bookstore.Domain.Model;

    namespace Bookstore.Infrastructure.Repositories.OpenDdd.Postgres
    {
        public class PostgresOpenDddCustomerRepository : PostgresOpenDddRepository<Customer, Guid>, ICustomerRepository
        {
            private readonly ILogger<PostgresOpenDddCustomerRepository> _logger;

            public PostgresOpenDddCustomerRepository(
                PostgresDatabaseSession session, 
                IAggregateSerializer serializer) 
                : base(session, serializer)
            {
                
            }

            public async Task<Customer> GetByEmailAsync(string email, CancellationToken ct)
            {
                // Implement your additional method..
            }
        }
    }

Using EF Core
-------------

By default, OpenDDD.NET uses its **custom persistence provider**, which follows a **document storage model**. This aligns closely with **DDD aggregate patterns** (including Alistair Cockburn’s **Entity pattern**) by storing aggregates **as serialized JSON documents**.

If you for some reason need **relational storage**, you can configure **EF Core** as the persistence provider. In that case, you must define:

- A subclass of `OpenDddDbContextBase`
- Subclasses of `EfAggregateRootConfigurationBase` for aggregates
- Subclasses of `EfEntityConfigurationBase` for entities
- Subclasses of `EfCoreRepository<TAggregateRoot, TId>` for custom repositories
- Use the `AddOpenDdd<TDbContext>` overload when registering OpenDDD to specify your custom DbContext

See the **Bookstore sample project** for examples.

Summary
-------

- Repositories implement `IRepository<TAggregateRoot, TId>`, ensuring a **consistent API**.
- Aggregates are stored as **JSON documents** in the configured database.
- **Auto-registration** registers repositories unless overridden by a custom interface.
- **Custom repositories** can be created by subclassing a base repository class.
- **EF Core** can be used instead by configuring it properly.

.. _building-blocks-actions-and-commands:

##################
Actions & Commands
##################

OpenDDD.NET separates **commands** (which represent an intent) from **actions** (which execute behavior). Actions drive domain logic by delegating to **aggregate roots** and/or **domain services**.

Commands
--------

A **Command** represents an explicit request to perform an operation. Commands do not return values and should not contain business logic.

.. code-block:: csharp

    using OpenDDD.Application;

    namespace Bookstore.Application.Actions.RegisterCustomer
    {
        public class RegisterCustomerCommand : ICommand
        {
            public string Name { get; set; }
            public string Email { get; set; }

            public RegisterCustomerCommand(string name, string email)
            {
                Name = name;
                Email = email;
            }
        }
    }

Actions
-------

An **Action** handles a command by executing the application logic. Actions are stateless and encapsulate high-level operations.

.. code-block:: csharp

    using OpenDDD.Application;
    using Bookstore.Domain.Model;
    using Bookstore.Domain.Service;

    namespace Bookstore.Application.Actions.RegisterCustomer
    {
        public class RegisterCustomerAction : IAction<RegisterCustomerCommand, Customer>
        {
            private readonly ICustomerDomainService _customerDomainService;

            public RegisterCustomerAction(ICustomerDomainService customerDomainService)
            {
                _customerDomainService = customerDomainService;
            }

            public async Task<Customer> ExecuteAsync(RegisterCustomerCommand command, CancellationToken ct)
            {
                var customer = await _customerDomainService.RegisterAsync(command.Name, command.Email, ct);
                return customer;
            }
        }
    }

.. _building-blocks-events:

######
Events
######

Events capture **state changes** in the domain and enable **decoupled communication**. OpenDDD.NET supports **Domain Events** and **Integration Events**.

Domain Events
-------------

A **Domain Event** represents a significant change within the domain.

**Defining a Domain Event:**

.. code-block:: csharp

    using OpenDDD.Domain.Model;

    public class CustomerRegistered : IDomainEvent
    {
        public Guid CustomerId { get; }
        public string Email { get; }

        public CustomerRegistered(Guid customerId, string email)
        {
            CustomerId = customerId;
            Email = email;
        }
    }

Integration Events
------------------

An **Integration Event** notifies external bounded contexts of domain changes. It is part of your **interchange context** project.

**Defining an Integration Event:**

.. code-block:: csharp

    using OpenDDD.Domain.Model;

    namespace Bookstore.Interchange.Model.Events
    {
        public class PersonUpdatedIntegrationEvent : IIntegrationEvent
        {
            public string Email { get; set; }
            public string FullName { get; set; }

            public PersonUpdatedIntegrationEvent(string email, string fullName)
            {
                Email = email;
                FullName = fullName;
            }
        }
    }

Publishing Events
-----------------

Events are published using `IDomainPublisher` (for domain events) or `IIntegrationPublisher` (for integration events).

**Publishing a Domain Event from a Domain Service:**

.. code-block:: csharp

    using OpenDDD.Domain.Model;
    using Bookstore.Domain.Model;
    using Bookstore.Domain.Model.Events;

    namespace Bookstore.Domain.Service
    {
        public class CustomerDomainService : ICustomerDomainService
        {
            private readonly ICustomerRepository _customerRepository;
            private readonly IDomainPublisher _domainPublisher;

            public CustomerDomainService(ICustomerRepository customerRepository, IDomainPublisher domainPublisher)
            {
                _customerRepository = customerRepository ?? throw new ArgumentNullException(nameof(customerRepository));
                _domainPublisher = domainPublisher ?? throw new ArgumentNullException(nameof(domainPublisher));
            }

            public async Task<Customer> RegisterAsync(string name, string email, CancellationToken ct)
            {
                if (string.IsNullOrWhiteSpace(name))
                    throw new ArgumentException("Customer name cannot be empty.", nameof(name));

                if (string.IsNullOrWhiteSpace(email))
                    throw new ArgumentException("Customer email cannot be empty.", nameof(email));
                
                var existingCustomer = await _customerRepository.FindByEmailAsync(email, ct);

                if (existingCustomer != null)
                    throw new InvalidOperationException($"A customer with the email '{email}' already exists.");

                var newCustomer = Customer.Create(name, email);

                await _customerRepository.SaveAsync(newCustomer, ct);

                var domainEvent = new CustomerRegistered(newCustomer.Id, newCustomer.Name, newCustomer.Email, DateTime.UtcNow);
                await _domainPublisher.PublishAsync(domainEvent, ct);

                return newCustomer;
            }
        }
    }

**Publishing a Domain Event from an Aggregate:**

.. code-block:: csharp

    using OpenDDD.Domain.Model;
    using OpenDDD.Domain.Model.Base;
    using Bookstore.Domain.Model.Events;

    namespace Bookstore.Domain.Model
    {
        public class Customer : AggregateRootBase<Guid>
        {
            public string Name { get; private set; }
            public string Email { get; private set; }

            // ...

            public Task ChangeNameAsync(string name, IDomainPublisher domainPublisher, CancellationToken ct)
            {
                Name = name;
                
                var domainEvent = new CustomerChangedName(Id, Name);
                await domainPublisher.PublishAsync(domainEvent, ct);
            }
        }
    }

**Publishing an Integration Event:**

Follow the same procedure to publish an integration event as you publish a domain event, but use the *IIntegrationPublisher* instead of the *IDomainPublisher*.

Listening to Events
-------------------

Event listeners handle **asynchronous reactions** to events. Derive from the base listener class and implement the ``HandleAsync`` method. This method must create the command corresponding to the **intent** of the reaction that you create by invoking the corresponding **action**.

**Defining an Event Listener:**

.. code-block:: csharp

    using OpenDDD.Infrastructure.Events.Base;
    using OpenDDD.Infrastructure.Events;
    using OpenDDD.API.Options;
    using OpenDDD.API.HostedServices;
    using Bookstore.Application.Actions.SendWelcomeEmail;
    using Bookstore.Domain.Model.Events;

    namespace Bookstore.Application.Listeners.Domain
    {
        public class CustomerRegisteredListener : EventListenerBase<CustomerRegistered, SendWelcomeEmailAction>
        {
            public CustomerRegisteredListener(
                IMessagingProvider messagingProvider,
                OpenDddOptions options,
                IServiceScopeFactory serviceScopeFactory,
                StartupHostedService startupService,
                ILogger<CustomerRegisteredListener> logger)
                : base(messagingProvider, options, serviceScopeFactory, startupService, logger) { }

            public override async Task HandleAsync(CustomerRegistered domainEvent, SendWelcomeEmailAction action, CancellationToken ct)
            {
                var command = new SendWelcomeEmailCommand(domainEvent.Email, domainEvent.Name);
                await action.ExecuteAsync(command, ct);
            }
        }
    }

Topic Configuration
-------------------

Event topics can be customized in `OpenDddOptions`:

.. code-block:: json

    "OpenDDD": {
        "Events": {
            "DomainEventTopic": "Bookstore.Domain.{EventName}",
            "IntegrationEventTopic": "Bookstore.Interchange.{EventName}",
            "ListenerGroup": "Default"
        }
    }

If you only have one bounded context, use *Domain* as middle part of the domain event topic template. If you have multiple contexts, use the name of the bounded context instead.

Example: Domain event topic templates when multiple bounded contexts

- Bookstore.Customer.CustomerCreated
- Bookstore.Order.OrderPlaced
- Bookstore.Tracking.TrackingUpdated

A **listener group** defines a set of **competing consumers** for a topic. Each event is delivered **at least once** to the group, with only one instance in the group processing it. Multiple listener groups can receive the same event independently.

Summary
-------

- **Domain Events** capture internal changes within aggregates.
- **Integration Events** notify external systems of changes.
- **Publishers** (`IDomainPublisher`, `IIntegrationPublisher`) send events.
- **Listeners** react to events asynchronously.
- **Topics** are configurable in `OpenDddOptions`.

.. _building-blocks-domain-services:

###############
Domain Services
###############

A **Domain Service** provides domain-specific logic that **does not fit within an aggregate**. Unlike application services (actions), domain services belong to the **domain layer** and contain **pure business logic**.

Domain services are typically used when:  

- The logic **does not belong to an aggregate (entities or value objects)**.  
- The operation involves **external dependencies** (e.g., repositories, external services) but is still **domain logic**.  
- Business rules need to be **shared across multiple use cases**.  

**Example domain service interface:**

.. code-block:: csharp

    using OpenDDD.Domain.Service;
    using Bookstore.Domain.Model;

    namespace Bookstore.Domain.Service
    {
        public interface ICustomerDomainService : IDomainService
        {
            Task<Customer> RegisterAsync(string name, string email, CancellationToken ct);
        }
    }

**Implementation:**

.. code-block:: csharp

    using OpenDDD.Domain.Model;
    using Bookstore.Domain.Model;
    using Bookstore.Domain.Model.Events;

    namespace Bookstore.Domain.Service
    {
        public class CustomerDomainService : ICustomerDomainService
        {
            private readonly ICustomerRepository _customerRepository;
            private readonly IDomainPublisher _domainPublisher;

            public CustomerDomainService(ICustomerRepository customerRepository, IDomainPublisher domainPublisher)
            {
                _customerRepository = customerRepository;
                _domainPublisher = domainPublisher;
            }

            public async Task<Customer> RegisterAsync(string name, string email, CancellationToken ct)
            {
                var existingCustomer = await _customerRepository.FindByEmailAsync(email, ct);

                if (existingCustomer != null)
                    throw new InvalidOperationException($"A customer with the email '{email}' already exists.");

                var newCustomer = Customer.Create(name, email);

                await _customerRepository.SaveAsync(newCustomer, ct);

                var domainEvent = new CustomerRegistered(newCustomer.Id, newCustomer.Name, newCustomer.Email, DateTime.UtcNow);
                await _domainPublisher.PublishAsync(domainEvent, ct);

                return newCustomer;
            }
        }
    }

**Key Characteristics of Domain Services:**

- They contain **domain logic** but **are not part of an aggregate**.
- They do **not manage state**; they operate on domain objects.
- They can depend on **repositories, domain events, or external services** via ports.

Domain services **should not** be used for:

- Simple operations that belong to an **aggregate root**.
- Coordinating application workflows (use **actions** instead).
- Infrastructure concerns like logging or email (use **infrastructure services**).

.. _building-blocks-infrastructure-services:

#######################
Infrastructure Services
#######################

An **Infrastructure Service** handles technical concerns that are **not part of the domain model**. These services provide access to external systems, such as databases, file storage, or system clocks.

Unlike domain services, **infrastructure services belong to the infrastructure layer** and are typically implemented using frameworks or third-party libraries.

Example infrastructure service interface:

.. code-block:: csharp

    using OpenDDD.Infrastructure.Service;

    namespace Bookstore.Infrastructure.Service.FileStorage
    {
        public interface IFileStorageService : IInfrastructureService
        {
            Task UploadFileAsync(string path, byte[] content, CancellationToken ct);
            Task<byte[]> DownloadFileAsync(string path, CancellationToken ct);
        }
    }

Implementation:

.. code-block:: csharp

    using Microsoft.Extensions.Logging;

    namespace Bookstore.Infrastructure.Service.FileStorage.Local
    {
        public class LocalFileStorageService : IFileStorageService
        {
            private readonly ILogger<LocalFileStorageService> _logger;

            public LocalFileStorageService(ILogger<LocalFileStorageService> logger)
            {
                _logger = logger;
            }

            public async Task UploadFileAsync(string path, byte[] content, CancellationToken ct)
            {
                await File.WriteAllBytesAsync(path, content, ct);
                _logger.LogInformation($"File uploaded: {path}");
            }

            public async Task<byte[]> DownloadFileAsync(string path, CancellationToken ct)
            {
                if (!File.Exists(path))
                    throw new FileNotFoundException("File not found", path);

                _logger.LogInformation($"File downloaded: {path}");
                return await File.ReadAllBytesAsync(path, ct);
            }
        }
    }

**Key Characteristics of Infrastructure Services:**

- They interact with **external systems** (e.g., file storage, system clocks, OS-level services).
- They are **stateless** and provide reusable technical functionality.
- They should **not contain business logic** (that belongs in domain services or aggregates).

Infrastructure services **should not** be used for:

- Business logic that belongs in **domain services**.
- Application coordination (handled by **actions**).
- External integrations that fit the **Ports & Adapters** pattern (e.g., email, payment gateways).

.. _building-blocks-ports-and-adapters:

################
Ports & Adapters
################

In OpenDDD.NET, **ports** are domain-defined interfaces for **external interactions**, while **adapters** implement those ports. This ensures **external dependencies** (e.g., email, payments, cloud storage) do not leak into the domain model.

Unlike **Infrastructure Services**, which handle **purely technical concerns**, **Ports & Adapters** are used when an **external interaction is part of the business domain**.

**When to Use Ports & Adapters**

- When integrating **external systems** that are relevant to the domain.
- When the implementation **should be swappable** (e.g., SMTP vs. SendGrid for email).
- When you want to **decouple the domain layer** from specific infrastructure details.

Ports
-----

A **Port** is an interface that defines how the domain interacts with an external dependency. The **domain layer depends on the port**, while the implementation is provided by an adapter.

Example **Email Port**:

.. code-block:: csharp

    using OpenDDD.Domain.Model.Ports;

    namespace Bookstore.Domain.Model.Ports
    {
        public interface IEmailPort : IPort
        {
            Task SendEmailAsync(string to, string subject, string body, CancellationToken ct);
        }
    }

Adapters
--------

An **Adapter** is a concrete implementation of a **Port** that integrates with an external system.

Example **SMTP Email Adapter**:

.. code-block:: csharp

    using Microsoft.Extensions.Options;
    using MimeKit;
    using MailKit.Net.Smtp;
    using Bookstore.Domain.Model.Ports;
    using Bookstore.Infrastructure.Adapters.Smtp.Options;

    namespace Bookstore.Infrastructure.Adapters.Smtp
    {
        public class SmtpEmailAdapter : IEmailPort
        {
            private readonly ILogger<SmtpEmailAdapter> _logger;

            public SmtpEmailAdapter(ILogger<SmtpEmailAdapter> logger)
            {
                _logger = logger;
            }

            public Task SendEmailAsync(string to, string subject, string body, CancellationToken ct)
            {
                _logger.LogInformation($"Sending email via SMTP to {to}: {subject}");
                return Task.CompletedTask; // Replace with actual SMTP implementation
            }
        }
    }

This allows adapters to be swapped without modifying domain logic. For example, `SendGridEmailAdapter` could replace `SmtpEmailAdapter` transparently.

**Summary**

- **Ports** define business-relevant external interactions.
- **Adapters** implement ports and provide infrastructure details.
- **Use Ports & Adapters** for external dependencies that are part of the domain.
- **Use Infrastructure Services** for purely technical concerns.

##########
Next Steps
##########

Now that you're familiar with the building blocks of OpenDDD.NET, you can explore the next steps:

- [:ref:`Getting Started Guide <userguide-getting-started>`] – Learn how to set up OpenDDD.NET in your project.
- [:ref:`Configuration Guide <config>`] – Customize persistence, messaging, and event handling.
- [`Bookstore Sample Project <https://github.com/runemalm/OpenDDD.NET/tree/master/samples/Bookstore>`_] – See a full example implementation.
- [`OpenDDD.NET Discussions <https://github.com/runemalm/OpenDDD.NET/discussions>`_] – Get involved to ask questions, share insights, and contribute.
