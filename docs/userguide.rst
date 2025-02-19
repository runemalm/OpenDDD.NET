.. note::

    OpenDDD.NET is currently in beta. Features and documentation are under active development and subject to change.

.. _userguide-getting-started:

###############
Getting Started
###############

There are multiple ways to start using OpenDDD.NET:

- **Use the Project Template** → Generate a new project using the :ref:`project template <userguide-project-template>` to get started quickly with the correct structure and initial configurations.
- **Setup From Scratch** → Follow the :ref:`step-by-step guide <userguide-step-by-step>` to create a project from scratch.
- **Explore the Sample Project** → Browse the `Bookstore Sample Project <https://github.com/runemalm/OpenDDD.NET/tree/master/samples/Bookstore>`_ on GitHub for a fully implemented example.

.. _userguide-project-template:

################
Project Template
################

The **OpenDDD.NET project template** provides a quick way to set up a new project with the necessary structure, configuration, and boilerplate code.

**Install the template package:**

.. code-block:: bash

    dotnet new install OpenDDD.NET-Templates

**Create a new project:**

.. code-block:: bash

    dotnet new opendddnet-sln --framework net8.0 -n YourProjectName

This generates a **YourProjectName** project in your current directory, preconfigured with best practices to get you started quickly.

Continue building your domain model by adding aggregates, domain services, listeners, etc. Refer to the :ref:`Building Blocks <building-blocks>` section for more information.

.. _userguide-step-by-step:

##################
Step-by-Step Guide
##################

Follow these steps to setup a new project using OpenDDD.NET from scratch:

-----------------------
1: Create a new project
-----------------------

Create a new ASP.NET Core Web API project using the .NET CLI:

.. code-block:: bash

    $ dotnet new webapi -n YourProjectName

----------------------
2: Install OpenDDD.NET
----------------------

Install the OpenDDD.NET framework:

.. code-block:: bash

    $ dotnet add package OpenDDD.NET --prerelease

.. note::

    The library requires ASP.NET Core 8 or higher.

--------------------
3: Edit `Program.cs`
--------------------

Add OpenDDD.NET services and middleware to your application in the `Program.cs` file:

.. code-block:: csharp

    using OpenDDD.API.Extensions;

    var builder = WebApplication.CreateBuilder(args);

    // Add OpenDDD Services
    builder.Services.AddOpenDDD(builder.Configuration);

    var app = builder.Build();

    // Use OpenDDD Middleware
    app.UseOpenDDD();

    app.Run();

---------------
4: Domain Layer
---------------

Create aggregates, entities, value objects, domain events, domain services, ports and repository interfaces to represent your domain model.

Example definitions:

.. code-block:: csharp

    using OpenDDD.Domain.Model.Base;

    namespace YourProjectName.Domain.Model
    {
        public class Customer : AggregateRootBase<Guid>
        {
            public string Name { get; private set; }
            public string Email { get; private set; }
            
            public Customer(Guid id, string name, string email) : base(id)
            {
                Name = name;
                Email = email;
            }

            public static Customer Create(string name, string email)
            {
                return new Customer(Guid.NewGuid(), name, email);
            }

            public void ChangeName(string name)
            {
                Name = name;
            }
        }
    }

.. code-block:: csharp

    using OpenDDD.Domain.Model;

    namespace YourProjectName.Domain.Model.Events
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

.. code-block:: csharp

    using OpenDDD.Domain.Model;

    namespace YourProjectName.Domain.Model
    {
        public interface ICustomerRepository : IRepository<Customer, Guid>
        {
            public Task<Customer?> FindByEmailAsync(string email, CancellationToken ct);
        }
    }

.. code-block:: csharp

    using OpenDDD.Domain.Service;
    using YourProjectName.Domain.Model;

    namespace YourProjectName.Domain.Service
    {
        public interface ICustomerDomainService : IDomainService
        {
            Task<Customer> RegisterAsync(string name, string email, CancellationToken ct);
        }
    }

.. code-block:: csharp

    using OpenDDD.Domain.Model;
    using OpenDDD.Domain.Model.Exception;
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
                    throw new EntityExistsException("Customer", $"email '{email}'");

                var newCustomer = Customer.Create(name, email);

                await _customerRepository.SaveAsync(newCustomer, ct);

                var domainEvent = new CustomerRegistered(newCustomer.Id, newCustomer.Name, newCustomer.Email, DateTime.UtcNow);
                await _domainPublisher.PublishAsync(domainEvent, ct);

                return newCustomer;
            }
        }
    }

.. code-block:: csharp

    using OpenDDD.Domain.Model.Ports;

    namespace YourProjectName.Domain.Model.Ports
    {
        public interface IEmailPort : IPort
        {
            Task SendEmailAsync(string to, string subject, string body, CancellationToken ct);
        }
    }

--------------------
5: Application Layer
--------------------

Create commands, actions and event listeners to handle application logic.

Example definitions:

.. code-block:: csharp

    using OpenDDD.Application;

    namespace YourProjectName.Application.Actions.RegisterCustomer
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

.. code-block:: csharp

    using OpenDDD.Application;
    using YourProjectName.Domain.Model;
    using YourProjectName.Domain.Service;

    namespace YourProjectName.Application.Actions.RegisterCustomer
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

.. code-block:: csharp

    using OpenDDD.Infrastructure.Events.Base;
    using OpenDDD.Infrastructure.Events;
    using OpenDDD.API.Options;
    using OpenDDD.API.HostedServices;
    using YourProjectName.Application.Actions.SendWelcomeEmail;
    using YourProjectName.Domain.Model.Events;

    namespace YourProjectName.Application.Listeners.Domain
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

.. code-block:: csharp

    using OpenDDD.Application;

    namespace YourProjectName.Application.Actions.SendWelcomeEmail
    {
        public class SendWelcomeEmailCommand : ICommand
        {
            public string RecipientEmail { get; set; }
            public string RecipientName { get; set; }

            public SendWelcomeEmailCommand(string recipientEmail, string recipientName)
            {
                RecipientEmail = recipientEmail;
                RecipientName = recipientName;
            }
        }
    }


.. code-block:: csharp

    using OpenDDD.Application;
    using YourProjectName.Domain.Model.Ports;

    namespace YourProjectName.Application.Actions.SendWelcomeEmail
    {
        public class SendWelcomeEmailAction : IAction<SendWelcomeEmailCommand, object>
        {
            private readonly IEmailPort _emailPort;

            public SendWelcomeEmailAction(IEmailPort emailPort)
            {
                _emailPort = emailPort;
            }

            public async Task<object> ExecuteAsync(SendWelcomeEmailCommand command, CancellationToken ct)
            {
                var subject = "Welcome to YourProjectName!";
                var body = $"Dear {command.RecipientName},\n\nThank you for registering with us. We're excited to have you on board!\n\n- YourProjectName Team";

                // Send email
                await _emailPort.SendEmailAsync(command.RecipientEmail, subject, body, ct);

                return new { };
            }
        }
    }

-----------------------
6: Infrastructure Layer
-----------------------

Create your repository implementation classes. Create adapter classes for the ports in your domain layer.

Example definitions:

.. code-block:: csharp

    using OpenDDD.Infrastructure.Persistence.OpenDdd.DatabaseSession.Postgres;
    using OpenDDD.Infrastructure.Repository.OpenDdd.Postgres;
    using OpenDDD.Infrastructure.Persistence.Serializers;
    using OpenDDD.Domain.Model.Exception;
    using YourProjectName.Domain.Model;

    namespace YourProjectName.Infrastructure.Repositories.OpenDdd.Postgres
    {
        public class PostgresOpenDddCustomerRepository : PostgresOpenDddRepository<Customer, Guid>, ICustomerRepository
        {
            private readonly ILogger<PostgresOpenDddCustomerRepository> _logger;

            public PostgresOpenDddCustomerRepository(
                PostgresDatabaseSession session, 
                IAggregateSerializer serializer, 
                ILogger<PostgresOpenDddCustomerRepository> logger) 
                : base(session, serializer)
            {
                _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            }

            public async Task<Customer> GetByEmailAsync(string email, CancellationToken ct = default)
            {
                var customer = await FindByEmailAsync(email, ct);
                return customer ?? throw new DomainException($"No customer found with email '{email}'.");
            }

            public async Task<Customer?> FindByEmailAsync(string email, CancellationToken ct = default)
            {
                if (string.IsNullOrWhiteSpace(email))
                {
                    throw new ArgumentException("Email cannot be null or whitespace.", nameof(email));
                }

                return (await FindWithAsync(c => c.Email == email, ct)).FirstOrDefault();
            }
        }
    }

.. code-block:: csharp

    using YourProjectName.Domain.Model.Ports;

    namespace YourProjectName.Infrastructure.Adapters.Console
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

Then register the port with the adapter class in `Program.cs` like this:

.. code-block:: csharp
    
    // ...

    // Add a custom adapter
    builder.Services.AddTransient<IEmailPort, ConsoleEmailAdapter>();

    var app = builder.Build();

    // ...

----------------------
7: Add Web API Adapter
----------------------

Create an http adapter for your application layer actions. We need to:

- Create a **controller** to open endpoints and invoke actions.
- Add **Controller-**, **Swagger-** and **API Explorer** services in `Program.cs`.
- Add **HTTPS Redirection-**, **CORS-** and **Swagger** middleware in `Program.cs`.
- Map controllers to endpoints in `Program.cs`.

Example definitions:

.. code-block:: csharp

    using Microsoft.AspNetCore.Mvc;
    using YourProjectName.Application.Actions.GetCustomer;
    using YourProjectName.Application.Actions.GetCustomers;
    using YourProjectName.Application.Actions.RegisterCustomer;
    using YourProjectName.Domain.Model;

    namespace YourProjectName.Infrastructure.Adapters.WebAPI.Controllers
    {
        [ApiController]
        [Route("api/customers")]
        public class CustomerController : ControllerBase
        {
            private readonly RegisterCustomerAction _registerCustomerAction;
            private readonly GetCustomerAction _getCustomerAction;
            private readonly GetCustomersAction _getCustomersAction;

            public CustomerController(
                RegisterCustomerAction registerCustomerAction,
                GetCustomerAction getCustomerAction,
                GetCustomersAction getCustomersAction)
            {
                _registerCustomerAction = registerCustomerAction;
                _getCustomerAction = getCustomerAction;
                _getCustomersAction = getCustomersAction;
            }

            [HttpPost("register-customer")]
            public async Task<ActionResult<Customer>> RegisterCustomer([FromBody] RegisterCustomerCommand command, CancellationToken ct)
            {
                try
                {
                    var customer = await _registerCustomerAction.ExecuteAsync(command, ct);
                    return CreatedAtAction(nameof(GetCustomer), new { id = customer.Id }, customer);
                }
                catch (Exception ex)
                {
                    return BadRequest(new { Message = ex.Message });
                }
            }
        }
    }

.. code-block:: csharp

    using OpenDDD.API.Extensions;
    using YourProjectName.Domain.Model.Ports;
    using YourProjectName.Infrastructure.Adapters.Console;

    var builder = WebApplication.CreateBuilder(args);

    // Add Swagger Services
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen();

    // Add OpenDDD services
    builder.Services.AddOpenDDD(builder.Configuration,
        options =>  
        {  
            options.UseInMemoryDatabase()
                   .UseInMemoryMessaging()
                   .SetEventListenerGroup("YourProjectName")
                   .SetEventTopics(
                       "YourProjectName.Domain.{EventName}",
                       "YourProjectName.Interchange.{EventName}"
                    )
                   .EnableAutoRegistration();
        },
        services =>
        {
            services.AddTransient<IEmailPort, ConsoleEmailAdapter>();
        }
    );

    // Add Controller Services
    builder.Services.AddControllers();

    // Build the application
    var app = builder.Build();

    // Use OpenDDD Middleware
    app.UseOpenDDD();

    // Use Swagger Middleware
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
        app.UseDeveloperExceptionPage();
    }

    // Use HTTP->HTTPS Redirection Middleware
    app.UseHttpsRedirection();

    // Use CORS Middleware
    app.UseCors("AllowAll");

    // Map Controller Actions to Endpoints
    app.MapControllers();

    // Run the application
    app.Run();

--------------------------
8: Edit `appsettings.json`
--------------------------

Add the following configuration to your `appsettings.json` file to customize OpenDDD.NET behavior:

.. code-block:: json

    "OpenDDD": {
      "PersistenceProvider": "OpenDDD",
      "DatabaseProvider": "InMemory",
      "MessagingProvider": "InMemory",
      "Events": {
        "DomainEventTopic": "YourProjectName.Domain.{EventName}",
        "IntegrationEventTopic": "YourProjectName.Interchange.{EventName}",
        "ListenerGroup": "Default"
      },
      "SQLite": {
        "ConnectionString": "DataSource=Infrastructure/Persistence/EfCore/YourProjectName.db;Cache=Shared"
      },
      "Postgres": {
        "ConnectionString": "Host=localhost;Port=5432;Database=yourprojectname;Username=your_username;Password=your_password"
      },
      "AzureServiceBus": {
        "ConnectionString": "",
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
        "BootstrapServers": "localhost:9092",
        "AutoCreateTopics": true
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

For all information about configuration, see :ref:`Configuration <config>`.

----------------------
9: Run the Application
----------------------

Now you are ready to run the application:

.. code-block:: bash

    dotnet run

To register a new customer, send a `POST` request to:

.. code-block:: none

    POST /api/customers/register-customer

Fill in the request body with:

.. code-block:: json

    {
      "name": "Alice",
      "email": "alice@example.com"
    }

Click **Execute** to run the request.

.. _userguide-sample-project:

##################
Run Sample Project
##################

The `Bookstore` sample project demonstrates how to build a **DDD-based** application using OpenDDD.NET.  
It includes **domain models, repositories, actions, and event-driven processing**.
Most of the example code in the documentation is taken from the sample project.

Find the source code here: `Bookstore Sample Project <https://github.com/runemalm/OpenDDD.NET/tree/master/samples/Bookstore>`_.

**Run the Sample:**

.. code-block:: bash

   git clone https://github.com/runemalm/OpenDDD.NET.git
   cd OpenDDD.NET/samples/Bookstore/src/Bookstore
   dotnet run

**Test the API:**

- **Register a customer** → `POST /api/customers/register-customer`
- Open **Swagger UI** at `http://localhost:5268/swagger` to explore and test endpoints.

##########
Next Steps
##########

- **Learn the Core Concepts** → The :ref:`Building Blocks <building-blocks>` section provides full documentation on each DDD building block in OpenDDD.NET.  
- **See a Full Implementation** → Explore the `Bookstore Sample Project <https://github.com/runemalm/OpenDDD.NET/tree/master/samples/Bookstore>`_ on GitHub.  
- **Get Involved** → Join the `OpenDDD.NET Discussions <https://github.com/runemalm/OpenDDD.NET/discussions>`_ to ask questions, share insights, and contribute.  
