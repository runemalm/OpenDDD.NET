###############
Getting Started
###############

.. note::

    OpenDDD.NET is currently in alpha. Features and documentation are under active development and subject to change.

############
Introduction
############

OpenDDD.NET is an open-source framework for building applications using Domain-Driven Design (DDD) principles in .NET. It provides a set of foundational tools and abstractions to help developers create scalable, maintainable, and testable software systems.

This guide will introduce you to the core concepts and show you how to start using OpenDDD.NET. For more detailed usage examples, refer to the `Building Blocks` section.

############
Installation
############

Install OpenDDD.NET using the .NET CLI:

.. code-block:: bash

    $ dotnet add package OpenDDD.NET --prerelease

The library requires .NET 8 or higher.

####################
Quick Start Overview
####################

To begin using OpenDDD.NET, follow these steps:

**Step 1: Configure Services in `Program.cs`**

Add OpenDDD.NET services to your application in the `Program.cs` file:

.. code-block:: csharp

    using OpenDDD.Main.Extensions;

    var builder = WebApplication.CreateBuilder(args);

    // Add OpenDDD Services
    builder.Services.AddOpenDDD(builder.Configuration);

    var app = builder.Build();

    // Add OpenDDD Middleware
    app.UseOpenDDD();

    app.Run();

**Step 2: Define Your Domain**

Create aggregates, entities, and value objects to represent your domain model. For example:

.. code-block:: csharp

    public class Order : AggregateRootBase<Guid>
    {
        public string CustomerName { get; private set; }

        public Order(Guid id, string customerName) : base(id)
        {
            CustomerName = customerName;
        }
    }

**Step 3: Implement Repositories, Actions, and Domain Services**

Define repositories for aggregates, create actions to handle application logic, and implement domain services for cross-aggregate domain logic.

Repositories are automatically registered based on naming conventions:

- Interfaces ending with `Repository` (e.g., `ICustomerRepository`) are automatically registered with their corresponding implementation (e.g., `PostgresCustomerRepository` or `InMemoryCustomerRepository`), depending on the configuration.

Actions are also auto-registered:

- Any class implementing ``IAction<TCommand, TReturns>`` is registered with transient scope.

Domain Services encapsulate domain-specific operations:

- Interfaces extending ``IDomainService`` (e.g., `ICustomerDomainService`) are automatically registered with their matching implementations (e.g., `CustomerDomainService`) by convention.

**Example definitions:**

.. code-block:: csharp

    // Repository interface
    public interface IOrderRepository : IRepository<Order, Guid>
    {
        Task<IEnumerable<Order>?> FindByCustomerNameAsync(string customerName);
    }

    // Action
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

    // Domain service
    public interface ICustomerDomainService : IDomainService
    {
        Task<Customer> Register(string name, string email);
    }

    public class CustomerDomainService : ICustomerDomainService
    {
        private readonly ICustomerRepository _customerRepository;

        public CustomerDomainService(ICustomerRepository customerRepository)
        {
            _customerRepository = customerRepository;
        }

        public async Task<Customer> Register(string name, string email)
        {
            var customer = new Customer(Guid.NewGuid(), name, email);
            await _customerRepository.SaveAsync(customer, default);
            return customer;
        }
    }

**Step 4: Add Configuration**

Add the following configuration to your `appsettings.json` file to customize OpenDDD.NET behavior:

.. code-block:: json

    {
      "OpenDDD": {
        "AutoRegisterDomainServices": true,
        "AutoRegisterRepositories": true,
        "AutoRegisterActions": true,
        "PersistenceProvider": "InMemory"
      }
    }

- **AutoRegisterDomainServices**: Registers domain service interfaces (e.g., `ICustomerDomainService`) with their implementations (e.g., `CustomerDomainService`).
- **AutoRegisterRepositories**: Automatically registers repository interfaces (e.g., `ICustomerRepository`) with their corresponding implementations (e.g., `PostgresCustomerRepository`).
- **AutoRegisterActions**: Enables automatic registration of all classes implementing `IAction<TCommand, TReturns>` with transient scope.
- **PersistenceProvider**: Define the persistence provider to use, (one of *InMemory*, *Postgres*).

#################
Where to Go Next?
#################

- **Explore Building Blocks**: Learn more about the foundational components of OpenDDD.NET in the `Building Blocks` section.
- **Contribute**: Join the OpenDDD.NET community on GitHub to report issues, ask questions, or contribute to the project.
