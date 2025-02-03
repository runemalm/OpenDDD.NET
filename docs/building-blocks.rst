.. note::

    OpenDDD.NET is currently in alpha. Features and documentation are under active development and subject to change.

.. _building-blocks:

########
Overview
########

OpenDDD.NET provides a set of **building blocks** to help structure applications using **Domain-Driven Design (DDD)**.  
Each block serves a specific purpose in organizing business logic, enforcing boundaries, and maintaining consistency.

.. _bb-aggregates:

##########
Aggregates
##########

An **Aggregate** is a cluster of related domain objects that are treated as a single unit.  
It ensures **data integrity** by enforcing business rules and encapsulating state changes.

Each aggregate has a single **Aggregate Root**, which is the only entry point for modifications.  
All external interactions must go through the aggregate root.

In OpenDDD.NET, aggregate roots subclass ``AggregateRootBase<TId>``.

.. code-block:: csharp

    public class Order : AggregateRootBase<Guid>
    {
        public string CustomerName { get; private set; }
        public List<OrderItem> Items { get; private set; } = new();

        public Order(Guid id, string customerName) : base(id)
        {
            CustomerName = customerName;
        }

        public void AddItem(OrderItem item)
        {
            Items.Add(item);
        }
    }

.. _bb-entities:

########
Entities
########

An **Entity** is a domain object with a unique identity that persists throughout its lifecycle.  
Entities typically belong to an aggregate and are referenced by their **ID**.

In OpenDDD.NET, entities subclass ``EntityBase<TId>``.

.. code-block:: csharp

    public class Product : EntityBase<Guid>
    {
        public string Name { get; private set; }

        public Product(Guid id, string name) : base(id)
        {
            Name = name;
        }
    }

.. _bb-value-objects:

#############
Value Objects
#############

A **Value Object** represents a domain concept that is **immutable** and **defined by its values**  
rather than an identity. Two value objects are considered equal if their properties have the same values.

In OpenDDD.NET, value objects implement the ``IValueObject`` marker interface.

.. code-block:: csharp

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

.. _bb-repositories:

############
Repositories
############

A **Repository** provides an abstraction for accessing and persisting aggregates.  
It acts as a collection-like interface for retrieving and storing **aggregates**, keeping the domain logic independent of the database.

------------------
Using Repositories
------------------

In OpenDDD.NET, repositories are implemented by subclassing a **base repository class**.  
The base class used depends on the configured **persistence provider**.

Currently, OpenDDD.NET supports **Entity Framework Core (EF Core)**,  
so repositories should subclass ``EfCoreRepository<TAggregateRoot, TId>``.

.. code-block:: csharp

    public class EfCoreOrderRepository : EfCoreRepository<Order, Guid>, IOrderRepository
    {
        public EfCoreOrderRepository(IUnitOfWork unitOfWork) : base(unitOfWork) { }
    }

If no custom repository implementation is provided, OpenDDD.NET will **auto-register** a generic repository.  
You can inject it using the ``IRepository<TAggregateRoot, TId>`` interface.

.. code-block:: csharp

    public class PlaceOrderAction : IAction<PlaceOrderCommand, Guid>
    {
        private readonly IRepository<Order, Guid> _orderRepository;

        public PlaceOrderAction(IRepository<Order, Guid> orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<Guid> ExecuteAsync(PlaceOrderCommand command, CancellationToken ct)
        {
            var order = new Order(Guid.NewGuid(), command.CustomerName);
            await _orderRepository.SaveAsync(order, ct);
            return order.Id;
        }
    }

------------------
Repository Methods
------------------

Repositories follow a **naming convention** for retrieving and modifying aggregates:

- **GetAsync(id)** → Retrieves a single aggregate by ID.  
  Throws an exception if **none or multiple** exist.  
- **FindAsync(id)** → Finds an aggregate by ID, returning `null` if not found.  
- **FindWithAsync(filter)** → Finds aggregates matching a **LINQ filter**.  
- **FindAllAsync()** → Retrieves **all aggregates** of the given type.  
- **SaveAsync(aggregate)** → **Upserts** an aggregate (creates or updates).  
- **DeleteAsync(aggregate)** → Deletes the given aggregate.

Example usage:

.. code-block:: csharp

    var order = await _orderRepository.GetAsync(orderId, ct);
    var orders = await _orderRepository.FindWithAsync(o => o.CustomerName == "Alice", ct);
    await _orderRepository.SaveAsync(new Order(Guid.NewGuid(), "Alice"), ct);

.. _bb-actions:

#######
Actions
#######

An **Action** is an application-layer component responsible for executing business logic in response to a **Command**.  
Actions coordinate domain operations but do not contain domain rules themselves.

------------------
Defining an Action
------------------

Actions in OpenDDD.NET subclass ``IAction<TCommand, TResponse>`` 
where `TCommand` represents the **input** and `TResponse` represents the **output**.

.. code-block:: csharp

    public class PlaceOrderAction : IAction<PlaceOrderCommand, Guid>
    {
        private readonly IRepository<Order, Guid> _orderRepository;

        public PlaceOrderAction(IRepository<Order, Guid> orderRepository)
        {
            _orderRepository = orderRepository;
        }

        public async Task<Guid> ExecuteAsync(PlaceOrderCommand command, CancellationToken ct)
        {
            var order = new Order(Guid.NewGuid(), command.CustomerName);
            await _orderRepository.SaveAsync(order, ct);
            return order.Id;
        }
    }

----------------------
Automatic Registration
----------------------

By default, OpenDDD.NET **automatically registers all Actions** for dependency injection,  
so they can be injected where needed.

To disable or modify this behavior, refer to the :ref:`configuration settings <config-auto-registration>`.

.. _bb-commands:

########
Commands
########

A **Command** represents a request to perform an operation on an **Aggregate**.  
Commands should express **intent** and clearly indicate the desired state change within the domain.  
They contain only **data** and should not implement business logic.

.. code-block:: csharp

    public class PlaceOrderCommand : ICommand
    {
        public string CustomerName { get; }

        public PlaceOrderCommand(string customerName)
        {
            CustomerName = customerName;
        }
    }

Commands are processed by **Actions**, which apply the requested change to the appropriate aggregate.

In OpenDDD.NET, commands implement the ``ICommand`` marker interface.

.. _bb-domain-events:

#############
Domain Events
#############

A **Domain Event** represents a significant change within an aggregate or domain service.  
They allow different parts of the domain to react asynchronously while maintaining **strong consistency** within an aggregate.

In OpenDDD.NET, domain events implement the ``IDomainEvent`` marker interface.

------------------------
Publishing Domain Events
------------------------

Domain events can be published from **aggregate roots** and **domain services**.

**1. From an Aggregate Root**  
To publish a domain event from an aggregate, pass the **domain event publisher** as an argument to the aggregate method.

.. code-block:: csharp

    public class Customer : AggregateRootBase<Guid>
    {
        public string Name { get; private set; }
        public string Email { get; private set; }

        private Customer() : base(Guid.Empty) { }

        public static Customer Register(string name, string email, IDomainPublisher domainPublisher)
        {
            var customer = new Customer(Guid.NewGuid(), name, email);
            var domainEvent = new CustomerRegistered(customer.Id, customer.Name, customer.Email, DateTime.UtcNow);
            domainPublisher.PublishAsync(domainEvent, CancellationToken.None);
            return customer;
        }
    }

**2. From a Domain Service**  
To publish a domain event from a domain service, inject ``IDomainPublisher`` into the constructor.

.. code-block:: csharp

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
            var customer = new Customer(Guid.NewGuid(), name, email);
            await _customerRepository.SaveAsync(customer, ct);

            var domainEvent = new CustomerRegistered(customer.Id, customer.Name, customer.Email, DateTime.UtcNow);
            await _domainPublisher.PublishAsync(domainEvent, ct);

            return customer;
        }
    }

-------------------------
Event Delivery and Topics
-------------------------

Domain events are **published on topics** according to **configured naming conventions**  
and are delivered using the **configured messaging provider**.

The supported messaging providers are:

- **In-Memory** (for local event handling)
- **Azure Service Bus** (for distributed event processing)

By default, topics are named using the format:

.. code-block:: json

    "Events": {
      "DomainEventTopicTemplate": "Bookstore.Domain.{EventName}"
    }

For businesses with **multiple bounded contexts**, replace `"Domain"` with the bounded context name:

.. code-block:: json

    "Events": {
      "DomainEventTopicTemplate": "Bookstore.Customer.{EventName}"
    }

For more details, see :ref:`Messaging Settings <config-messaging>`.

.. _bb-integration-events:

##################
Integration Events
##################

An **Integration Event** represents a **business event** that is intended for communication **across bounded contexts**.  
Unlike domain events, which model **internal state changes**, integration events are part of the **Interchange bounded context**  
and are used to notify external systems or other bounded contexts about important business events.

Integration events implement the ``IIntegrationEvent`` marker interface.

-----------------------------
Publishing Integration Events
-----------------------------

Integration events can be published from **aggregate roots** and **domain services**, just like domain events.

**1. From an Aggregate Root**  
To publish an integration event from an aggregate, pass the **integration event publisher** as an argument to the aggregate method.

.. code-block:: csharp

    public class Order : AggregateRootBase<Guid>
    {
        public string CustomerName { get; private set; }

        private Order() : base(Guid.Empty) { }

        public static Order Place(Guid orderId, string customerName, IIntegrationPublisher integrationPublisher)
        {
            var order = new Order(orderId, customerName);
            var integrationEvent = new OrderCreatedIntegrationEvent(order.Id, order.CustomerName);
            integrationPublisher.PublishAsync(integrationEvent, CancellationToken.None);
            return order;
        }
    }

**2. From a Domain Service**  
To publish an integration event from a domain service, inject ``IIntegrationPublisher`` into the constructor.

.. code-block:: csharp

    public class CustomerDomainService : ICustomerDomainService
    {
        private readonly ICustomerRepository _customerRepository;
        private readonly IDomainPublisher _domainPublisher;
        private readonly IIntegrationPublisher _integrationPublisher;

        public CustomerDomainService(
            ICustomerRepository customerRepository, 
            IDomainPublisher domainPublisher, 
            IIntegrationPublisher integrationPublisher)
        {
            _customerRepository = customerRepository;
            _domainPublisher = domainPublisher;
            _integrationPublisher = integrationPublisher;
        }

        public async Task<Customer> RegisterAsync(string name, string email, CancellationToken ct)
        {
            var customer = new Customer(Guid.NewGuid(), name, email);
            await _customerRepository.SaveAsync(customer, ct);

            var domainEvent = new CustomerRegistered(customer.Id, customer.Name, customer.Email, DateTime.UtcNow);
            await _domainPublisher.PublishAsync(domainEvent, ct);

            var integrationEvent = new PersonUpdatedIntegrationEvent(customer.Email, customer.Name);
            await _integrationPublisher.PublishAsync(integrationEvent, ct);

            return customer;
        }
    }

-------------------------
Event Delivery and Topics
-------------------------

Integration events are **published on topics** configured in `appsettings.json`  
and are delivered using the **configured messaging provider**.

Since integration events always belong to the **Interchange bounded context**,  
their topic naming convention always includes `"Interchange"` as the middle part:

.. code-block:: json

    "Events": {
      "IntegrationEventTopicTemplate": "Bookstore.Interchange.{EventName}"
    }

This is unlike domain event topics which always have either `"Domain"` (single bounded context) or the name of the **bounded context** as middle part:

The supported messaging providers are:

- **In-Memory** (for simple event propagation within the same process)
- **Azure Service Bus** (for event-driven communication across distributed services)

For messaging setup and configuration, see :ref:`Messaging Settings <config-messaging>`.

.. _bb-event-listeners:

###############
Event Listeners
###############

Event listeners in OpenDDD.NET handle **domain events** and **integration events** asynchronously.  
They decouple event processing by allowing different parts of the system to react to changes  
without direct dependencies.

------------------------------
Implementing an Event Listener
------------------------------

To create an event listener, subclass ``EventListenerBase<TEvent, TAction>``  
and implement the `HandleAsync` method to execute the associated **action**.

Example: A listener that sends a welcome email when a **CustomerRegistered** domain event is received.

.. code-block:: csharp

    using OpenDDD.Infrastructure.Events.Base;
    using OpenDDD.API.Options;
    using OpenDDD.Infrastructure.Events;
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
                ILogger<CustomerRegisteredListener> logger)
                : base(messagingProvider, options, serviceScopeFactory, logger) { }

            public override async Task HandleAsync(CustomerRegistered domainEvent, SendWelcomeEmailAction action, CancellationToken ct)
            {
                var command = new SendWelcomeEmailCommand(domainEvent.Email, domainEvent.Name);
                await action.ExecuteAsync(command, ct);
            }
        }
    }

Both **domain event listeners** and **integration event listeners** follow the same structure,  
only differing in the event type they handle.

.. _bb-domain-services:

###############
Domain Services
###############

A **Domain Service** encapsulates domain logic that does not naturally belong to a single aggregate.  
It is used when an operation requires **business logic that spans multiple aggregates**  
or depends on **domain-wide policies** that cannot be placed inside a single aggregate.  

A domain service must **only update one aggregate per operation**.
If changes to multiple aggregates are needed, it should **coordinate workflows using domain events** to maintain **eventual consistency** across aggregates.

- The domain service can **query multiple aggregates** to make decisions.
- Updates to multiple aggregates should be **triggered using domain events**
  to ensure **eventual consistency** rather than transactional consistency.

In OpenDDD.NET, domain services implement the ``IDomainService`` marker interface.  
They are **auto-registered by default**, but this behavior can be changed in configuration.  
See :ref:`Auto-Registration <config-general-auto-registration>` for details.  

-----------------------------
Implementing a Domain Service
-----------------------------

To define a domain service, create a class that implements ``IDomainService``  
and encapsulates the necessary domain logic.  

Example: A domain service that registers a new customer.  

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
                if (string.IsNullOrWhiteSpace(name))
                    throw new ArgumentException("Customer name cannot be empty.", nameof(name));

                if (string.IsNullOrWhiteSpace(email))
                    throw new ArgumentException("Customer email cannot be empty.", nameof(email));

                var existingCustomer = await _customerRepository.FindByEmailAsync(email, ct);
                if (existingCustomer != null)
                    throw new InvalidOperationException($"A customer with the email '{email}' already exists.");

                var customer = new Customer(Guid.NewGuid(), name, email);
                await _customerRepository.SaveAsync(customer, ct);

                var domainEvent = new CustomerRegistered(customer.Id, customer.Name, customer.Email, DateTime.UtcNow);
                await _domainPublisher.PublishAsync(domainEvent, ct);

                return customer;
            }
        }
    }

.. _bb-infrastructure-services:

#######################
Infrastructure Services
#######################

An **Infrastructure Service** provides technical capabilities that support the application  
but do not belong to the domain model.  

In OpenDDD.NET, infrastructure services are primarily used **internally by the framework**  
for features like **event handling, persistence, and messaging**.  

In a typical application, port **adapters** are preferred over infrastructure services  
for handling **external integrations**.  

However, an adapter **can also be an infrastructure service**.
By convention, OpenDDD.NET first classifies such components as **adapters** 
but know that they could be both.

In OpenDDD.NET, infrastructure services implement the ``IInfrastructureService`` marker interface.  
They are **auto-registered by default**, but this behavior can be changed in configuration.  
See :ref:`Auto-Registration <config-general-auto-registration>` for details.  

--------------------------------------
Implementing an Infrastructure Service
--------------------------------------

To define an infrastructure service, create a class that encapsulates the required functionality.  

Example: A logging service used for diagnostics.  

.. code-block:: csharp

    public class FileLoggerService : IInfrastructureService
    {
        private readonly string _logFilePath;

        public FileLoggerService(string logFilePath)
        {
            _logFilePath = logFilePath;
        }

        public void Log(string message)
        {
            File.AppendAllText(_logFilePath, $"{DateTime.UtcNow}: {message}\n");
        }
    }

.. _bb-ports-and-adapters:

################
Ports & Adapters
################

The **Ports & Adapters** (Hexagonal Architecture) pattern separates the **core domain logic**  
from **external dependencies** by defining **ports** (interfaces) and **adapters** (implementations).  
This ensures that the domain remains **decoupled** from infrastructure concerns.

---------------
Defining a Port
---------------

A **Port** is an interface that represents an external dependency, such as messaging, databases,  
or third-party services.

In OpenDDD.NET, ports implement the ``IPort`` marker interface.

Example: A port for sending emails.

.. code-block:: csharp

    public interface IEmailPort : IPort
    {
        Task SendEmailAsync(string to, string subject, string body, CancellationToken ct);
    }

-----------------------
Implementing an Adapter
-----------------------

An **Adapter** is an implementation of a port that interacts with a specific technology or service.

The adapter implements the relevant port interface.

Example: A console-based email adapter.

.. code-block:: csharp

    using Bookstore.Domain.Model.Ports;

    public class ConsoleEmailAdapter : IEmailPort
    {
        private readonly ILogger<ConsoleEmailAdapter> _logger;

        public ConsoleEmailAdapter(ILogger<ConsoleEmailAdapter> logger)
        {
            _logger = logger;
        }

        public Task SendEmailAsync(string to, string subject, string body, CancellationToken ct)
        {
            _logger.LogInformation($"Sending email to {to}: {subject}\n{body}");
            return Task.CompletedTask;
        }
    }

----------------------
Registering an Adapter
----------------------

Adapters are registered in `Program.cs` so they can be injected into application services.

.. code-block:: csharp

    builder.Services.AddTransient<IEmailPort, ConsoleEmailAdapter>();
