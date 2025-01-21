.. note::

    OpenDDD.NET is currently in alpha. Features and documentation are under active development and subject to change.

##########
Aggregates
##########

Aggregates represent the core units of your domain model, defining boundaries within which domain logic and consistency are enforced. Aggregates have a single root entity known as the *Aggregate Root*.

.. code-block:: csharp

    public class Order : AggregateRootBase<Guid>
    {
        public string CustomerName { get; private set; }

        public Order(Guid id, string customerName) : base(id)
        {
            CustomerName = customerName;
        }
    }

########
Entities
########

Entities are domain objects with unique identities that remain consistent throughout their lifecycle. They often make up the components of an aggregate.

.. code-block:: csharp

    public class Product : EntityBase<Guid>
    {
        public string Name { get; private set; }

        public Product(Guid id, string name) : base(id)
        {
            Name = name;
        }
    }

#############
Value Objects
#############

Value Objects represent immutable domain concepts without identity. They are defined by their values and are interchangeable when their values are the same.

.. code-block:: csharp

    public class Money : IValueObject
    {
        public decimal Amount { get; private set; }
        public string Currency { get; private set; }

        public Money(decimal amount, string currency)
        {
            Amount = amount;
            Currency = currency;
        }
    }

############
Repositories
############

Repositories in OpenDDD.NET abstract persistence concerns, enabling interaction with aggregates and entities while keeping domain logic clean. They allow you to interact with the database or other persistence mechanisms without introducing infrastructure-specific logic into the domain model.

Repositories are auto-registered in OpenDDD.NET. By default, implementations follow a convention-based registration:

Interfaces ending with *Repository* (e.g., `ICustomerRepository`) are automatically registered with their matching implementations.

The framework supports base classes for each persistence provider (e.g., ``PostgresRepositoryBase``, ``InMemoryRepositoryBase``). Use it when you build your custom repository classes.

**Example:**

Define a repository interface and a concrete implementation:

.. code-block:: csharp

    // Repository interface
    public interface ICustomerRepository : IRepository<Customer, Guid>
    {
        Customer GetByEmail(string email, CancellationToken ct = default);
    }

    // Postgres implementation
    public class PostgresCustomerRepository : PostgresRepositoryBase<Customer, Guid>, ICustomerRepository
    {
        public PostgresCustomerRepository(ILogger<PostgresCustomerRepository> logger) : base(logger) { }

        public Customer GetByEmail(string email, CancellationToken ct = default)
        {
            throw new NotImplementedException();
        }
    }

With auto-registration enabled, the repository is registered at application startup with the appropriate implementation, based on the configured persistence provider (e.g., **Postgres** or **InMemory**).

**Key Features:**

- **Convention-based registration:** Eliminates the need for manual configuration.
- **Flexible implementations:** Switch between different implementations (e.g., *Postgres* for production and *InMemory* for testing) by changing a configuration key.

#######
Actions
#######

Application *Actions* coordinate the execution of domain logic in response to commands. They are central to the application layer.

.. code-block:: csharp

    public class PlaceOrderAction : IAction<PlaceOrderCommand, Guid>
    {
        private readonly IOrderRepository _orderRepository;

        public PlaceOrderAction(IOrderRepository orderRepository)
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

#############
Domain Events
#############

Domain Events facilitate communication between domain objects while maintaining loose coupling. This feature is currently under development.

##################
Integration Events
##################

Integration Events enable communication between bounded contexts in distributed systems. This feature is currently under development.

###############
Event Listeners
###############

Event Listeners manage domain and integration events, supporting scalable, event-driven architectures. This feature is currently under development.

###############
Domain Services
###############

Domain Services encapsulate domain-specific operations that do not naturally belong to an entity or value object. They implement ``IDomainService`` or an interface extending it and provide operations that may cross multiple aggregates or entities.

Domain Services are auto-registered using a convention-based mechanism:

Interfaces implementing ``IDomainService`` (e.g., `ICustomerDomainService`) are automatically registered with their matching implementation (e.g., `CustomerDomainService`).

The default registration lifetime is **Transient**. You can override this using the ``LifetimeAttribute``.

**Example:**

Define a domain service interface and its implementation:

.. code-block:: csharp

    // Domain service interface
    public interface ICustomerDomainService : IDomainService
    {
        Task<Customer> Register(string name, string email);
    }

    // Implementation
    [Lifetime(ServiceLifetime.Singleton)] // Optional: Specify a custom lifetime
    public class CustomerDomainService : ICustomerDomainService
    {
        private readonly ICustomerRepository _customerRepository;

        public CustomerDomainService(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<Customer> Register(string name, string email)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be empty", nameof(name));

            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be empty", nameof(email));

            var existingCustomer = await _customerRepository.FindWithAsync(c => c.Email == email, default);
            if (existingCustomer.Any())
                throw new InvalidOperationException($"A customer with the email '{email}' already exists.");

            var customer = new Customer(Guid.NewGuid(), name, email);
            await _customerRepository.SaveAsync(customer, default);

            return customer;
        }
    }

**Key Features:**

- **Encapsulation:** Encapsulates domain logic that spans multiple entities or aggregates.
- **Auto-registration:** Automatically registers domain services with the DI container.
- **Customizable scope:** Use the ``LifetimeAttribute`` to override the default transient scope.

#######################
Infrastructure Services
#######################

Infrastructure Services provide implementations for technical concerns such as logging, email, or external integrations. This feature is currently under development.

####################
Transactional Outbox
####################

The Transactional Outbox ensures event consistency by persisting and publishing events as part of database transactions. This feature is currently under development.

---

Explore these building blocks in your own projects to unlock the full potential of OpenDDD.NET and simplify the implementation of DDD principles.
