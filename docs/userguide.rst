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

    // Register OpenDDD Services
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

**Step 3: Implement Repositories and Actions**

Define repositories for aggregates and create actions to handle application logic. For example:

.. code-block:: csharp

    public interface IOrderRepository : IRepository<Order, Guid>
    {
        Task<Order?> GetByCustomerNameAsync(string customerName);
    }

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

**Step 4: Add Configuration**

Add the following configuration to your `appsettings.json` file to customize OpenDDD.NET behavior:

.. code-block:: json

    {
      "OpenDDD": {
        "Services": {
          "AutoRegisterActions": true,
          "AutoRegisterRepositories": true
        },
        "Pipeline": {
          
        }
      }
    }

#################
Where to Go Next?
#################

- **Explore Building Blocks**: Learn more about the foundational components of OpenDDD.NET in the `Building Blocks` section.
- **Contribute**: Join the OpenDDD.NET community on GitHub to report issues, ask questions, or contribute to the project.
