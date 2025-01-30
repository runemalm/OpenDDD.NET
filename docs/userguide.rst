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

------------------------------------------
Step 1: Configure Services in `Program.cs`
------------------------------------------

Add OpenDDD.NET services to your application in the `Program.cs` file:

.. code-block:: csharp

    using OpenDDD.Main.Extensions;
    using Bookstore.Domain.Model.Ports;
    using Bookstore.Infrastructure.Adapters.Console;
    using Bookstore.Infrastructure.Persistence.EfCore;

    var builder = WebApplication.CreateBuilder(args);

    // Add OpenDDD Services
    builder.Services.AddOpenDDD<BookstoreDbContext>(builder.Configuration);

    var app = builder.Build();

    // Use OpenDDD Middleware
    app.UseOpenDDD();

    app.Run();

------------------------------
Step 2: Define Your Aggregates
------------------------------

Create aggregates, entities and value objects to represent your domain model.

Example definition:

.. code-block:: csharp

    using OpenDDD.Domain.Model.Base;

    namespace Bookstore.Domain.Model
    {
        public class Customer : AggregateRootBase<Guid>
        {
            public string Name { get; private set; }
            public string Email { get; private set; }
            
            private Customer() : base(Guid.Empty) { }

            public Customer(Guid id, string name, string email) : base(id)
            {
                Name = name;
                Email = email;
            }

            public void ChangeName(string name)
            {
                Name = name;
            }
        }
    }

----------------------------
Step 3: Define Domain Events
----------------------------

Create your events representing key domain actions.

Example definition:

.. code-block:: csharp

    using OpenDDD.Domain.Model;

    namespace Bookstore.Domain.Model.Events
    {
        public class CustomerRegistered : IDomainEvent
        {
            public Guid CustomerId { get; set; }
            public string Name { get; set; }
            public string Email { get; set; }
            public DateTime RegisteredAt { get; set; }
            
            public CustomerRegistered() { }

            public CustomerRegistered(Guid customerId, string name, string email, DateTime registeredAt)
            {
                CustomerId = customerId;
                Name = name;
                Email = email;
                RegisteredAt = registeredAt;
            }

            public override string ToString()
            {
                return $"CustomerRegistered: CustomerId={CustomerId}, Name={Name}, Email={Email}, RegisteredAt={RegisteredAt}";
            }
        }
    }

----------------------------
Step 4: Implement Repositories
----------------------------

Define repositories for aggregates.

Example definitions:

.. code-block:: csharp

    using OpenDDD.Domain.Model;

    namespace Bookstore.Domain.Model
    {
        public interface ICustomerRepository : IRepository<Customer, Guid>
        {
            public Task<Customer?> FindByEmailAsync(string email, CancellationToken ct = default);
        }
    }

.. code-block:: csharp

    using Microsoft.EntityFrameworkCore;
    using OpenDDD.Infrastructure.Persistence.UoW;
    using OpenDDD.Infrastructure.Repository.EfCore;
    using Bookstore.Domain.Model;

    namespace Bookstore.Infrastructure.Repositories.EfCore
    {
        public class EfCoreCustomerRepository : EfCoreRepository<Customer, Guid>, ICustomerRepository
        {
            private readonly ILogger<EfCoreCustomerRepository> _logger;

            public EfCoreCustomerRepository(IUnitOfWork unitOfWork, ILogger<EfCoreCustomerRepository> logger) 
                : base(unitOfWork)
            {
                _logger = logger;
            }
            
            public async Task<Customer?> FindByEmailAsync(string email, CancellationToken ct)
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    throw new ArgumentException("Email cannot be null or whitespace.", nameof(email));
                }

                return await DbContext.Set<Customer>()
                    .FirstOrDefaultAsync(c => EF.Functions.Like(c.Email, email), cancellationToken: ct);
            }
        }
    }

----------------------------
Step 5: Implement Actions and Commands
----------------------------

Create actions and their commands to handle application logic.

Example definitions:

.. code-block:: csharp

    using OpenDDD.Application;

    namespace Bookstore.Application.Actions.RegisterCustomer
    {
        public class RegisterCustomerCommand : ICommand
        {
            public string Name { get; set; }
            public string Email { get; set; }

            public RegisterCustomerCommand() { }

            public RegisterCustomerCommand(string name, string email)
            {
                Name = name;
                Email = email;
            }
        }
    }

.. code-block:: csharp

    using Bookstore.Domain.Model;
    using Bookstore.Domain.Service;
    using OpenDDD.Application;

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
                if (string.IsNullOrWhiteSpace(command.Name))
                    throw new ArgumentException("Customer name cannot be empty.", nameof(command.Name));

                if (string.IsNullOrWhiteSpace(command.Email))
                    throw new ArgumentException("Customer email cannot be empty.", nameof(command.Email));

                // Delegate the registration logic to the domain service
                var customer = await _customerDomainService.RegisterAsync(command.Name, command.Email, ct);
                return customer;
            }
        }
    }

----------------------------
Step 6: Implement Domain Services
----------------------------

Implement domain services for cross-aggregate domain logic.

Example definitions:

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

                var newCustomer = new Customer(Guid.NewGuid(), name, email);

                await _customerRepository.SaveAsync(newCustomer, ct);

                var domainEvent = new CustomerRegistered(newCustomer.Id, newCustomer.Name, newCustomer.Email, DateTime.UtcNow);
                await _domainPublisher.PublishAsync(domainEvent, ct);

                return newCustomer;
            }
        }
    }

----------------------------
Step 7: Implement Event Listeners
----------------------------

Event listeners in OpenDDD.NET process **domain events** and **integration events** asynchronously.  
They allow decoupled event-driven workflows where different parts of the application react to changes in the domain.

**1. Create an Event Listener**

    An event listener subscribes to an event and executes an **action** when the event is received.

    Example: A listener that sends a welcome email when a customer is registered.

    .. code-block:: csharp

        using OpenDDD.Infrastructure.Events.Base;
        using OpenDDD.Main.Options;
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

**2. Process Events Using Actions**

    Each event listener is paired with an **action** that contains the logic for handling the event. 

    Example: An action that sends an email.

    .. code-block:: csharp

        using OpenDDD.Application;
        using Bookstore.Domain.Model.Ports;

        namespace Bookstore.Application.Actions.SendWelcomeEmail
        {
            public class SendWelcomeEmailAction : IAction<SendWelcomeEmailCommand, object>
            {
                private readonly IEmailPort _emailPort;

                public SendWelcomeEmailAction(IEmailPort emailPort)
                {
                    _emailPort = emailPort ?? throw new ArgumentNullException(nameof(emailPort));
                }

                public async Task<object> ExecuteAsync(SendWelcomeEmailCommand command, CancellationToken ct)
                {
                    if (string.IsNullOrWhiteSpace(command.RecipientEmail))
                        throw new ArgumentException("Recipient email cannot be empty.", nameof(command.RecipientEmail));

                    if (string.IsNullOrWhiteSpace(command.RecipientName))
                        throw new ArgumentException("Recipient name cannot be empty.", nameof(command.RecipientName));

                    var subject = "Welcome to Bookstore!";
                    var body = $"Dear {command.RecipientName},\n\nThank you for registering with us. We're excited to have you on board!\n\n- Bookstore Team";

                    // Send email
                    await _emailPort.SendEmailAsync(command.RecipientEmail, subject, body, ct);

                    return new { };
                }
            }
        }

----------------------------
Step 8: Register port adapters
----------------------------

Port adapters in OpenDDD.NET allow your application to interact with external systems, such as **email services, payment gateways, or external APIs**.  
They implement **input and output ports**, enabling a clean separation of concerns.

**1. Define a Port Interface**

    A port defines the contract for an external dependency.

    Example: **IEmailPort** for sending emails.

    .. code-block:: csharp

        using OpenDDD.Domain.Model.Ports;

        namespace Bookstore.Domain.Model.Ports
        {
            public interface IEmailPort : IPort
            {
                Task SendEmailAsync(string to, string subject, string body, CancellationToken ct);
            }
        }

**2. Implement the Adapter**

    Adapters provide concrete implementations of the port interface.

    Example: A **console-based email adapter** for testing.

    .. code-block:: csharp

        using Bookstore.Domain.Model.Ports;

        namespace Bookstore.Infrastructure.Adapters.Console
        {
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
        }

**3. Register the Adapter in `Program.cs`**

    Register the adapter in **dependency injection (DI)**.

    .. code-block:: csharp

        builder.Services.AddTransient<IEmailPort, ConsoleEmailAdapter>();

**4. Use the Port in an Action or Service**

    You can see how the port is used in the ``SendWelcomeEmailAction`` above.

Port adapters make it easy to swap implementations, keeping the **domain layer** independent from external services.

----------------------------
Step 9: Add Configuration
----------------------------

Add the following configuration to your `appsettings.json` file to customize OpenDDD.NET behavior:

.. code-block:: json

    "OpenDDD": {
      "PersistenceProvider": "EfCore",
      "EfCore": {
        "Database": "SQLite",
        "ConnectionString": "DataSource=Main/EfCore/Bookstore.db;Cache=Shared"
      },
      "MessagingProvider": "InMemory",
      "Events": {
        "DomainEventTopic": "Bookstore.Domain.{EventName}",
        "IntegrationEventTopic": "Bookstore.Interchange.{EventName}",
        "ListenerGroup": "Default"
      },
      "AzureServiceBus": {
        "ConnectionString": "Endpoint=sb://your-servicebus.servicebus.windows.net/;SharedAccessKeyName=your-key;SharedAccessKey=your-key",
        "AutoCreateTopics": true
      },
      "AutoRegister": {
        "Actions": true,
        "DomainServices": true,
        "Repositories": true,
        "InfrastructureServices": true,
        "EventListeners": true,
        "EfCoreConfigurations": true
      }
    }

##############
Sample Project
##############

The `Bookstore` sample project demonstrates how to build a **DDD-based** application using OpenDDD.NET.  
It includes **domain models, repositories, actions, and event-driven processing**.
All the example code from the guide above were taken from the sample project.

Find the source code here: `Bookstore Sample Project <https://github.com/runemalm/OpenDDD.NET/tree/master/samples/Bookstore>`_.

**Run the Sample:**

.. code-block:: bash

   git clone https://github.com/runemalm/OpenDDD.NET.git
   cd OpenDDD.NET/samples/Bookstore
   dotnet run

**Test the API:**

- **Register a customer** → `POST /api/customers/register-customer`
- Open **Swagger UI** at `http://localhost:5000/swagger` (or the correct port) to explore and test endpoints.

#################
Where to Go Next?
#################

- **Explore Building Blocks**: Learn more about the foundational components of OpenDDD.NET in the `Building Blocks` section.
- **Sample Project**: Check out the sample project mentioned above.
- **Contribute**: Join the OpenDDD.NET community on GitHub to report issues, ask questions, or contribute to the project.
