## OpenDDD .NET

This is a framework for domain-driven design (DDD) using C# and .NET Core.
Star and/or follow the project to get notifications on new releases.

### Purpose

Domain-driven design is an approach to software development where focus lies on an ever-evolving and up-to-date domain model.

By focusing on the domain model, the software is kept up-to-date with the business language. This in turns means that legacy code is kept to a constant minimum. The code will be kept simple and easily maintained by a small team. The production rate is kept at a high.

Of course there are challenges to domain-driven design. This framework aims to support those challenges by providing a tool crafted specifically for the purpose.

If you're an avid practitioner of domain-driven design, please drop me a line if you're interested in becoming a contributor.

### Key Features

- Domain model versioning.
- HTTP API SemVer2.0 support.
- Swagger documentation auto-generation.
- Code generation from domain yaml file.
- Domain & integration events publishing.
- Event listeners.
- Migration support.
- Clear hexagonal architecture.
- Simple design.
- Testing framework (xUnit).
- Extensible architecture (ports- & adapters).
- Full code examples.
- Clean code.

### Ports & Adapters

The list of ports and the implementations implementing them are constantly growing. 

Among the adapters we currently support are PostgreSQL, RabbitMQ, Azure ServiceBus & Application Insights, MailChimp, Sendgrid and more.

If you find there's a specific technology you'd like to see as an adapter in the framework, please leave a recommendation, or perhaps a pull request. All ports have a clearly defined interface and the documentation contains [a section on implementing ports](...).

### Design Patterns

The following are the framework's foundational design patterns:

- [Domain-Driven Design](https://www.amazon.com/Domain-Driven-Design-Tackling-Complexity-Software/dp/0321125215)  
- [Hexagonal Architecture](https://alistair.cockburn.us/hexagonal-architecture/)
- [Event-Carried State Transfer](https://martinfowler.com/articles/201701-event-driven.html)
- [Near-infinite Scalability](https://queue.acm.org/detail.cfm?id=3025012)
- [xUnit](https://en.wikipedia.org/wiki/XUnit)
- [Expand and Contract](https://martinfowler.com/bliki/ParallelChange.html)
- [Env files](https://12factor.net/config)

Big thanks to Eric Evans for his [seminal book on DDD](https://www.amazon.com/Domain-Driven-Design-Tackling-Complexity-Software/dp/0321125215) and 
Vaughn Vernon for his [reference implementation](https://github.com/VaughnVernon/IDDD_Samples) of DDD in Java.

### Supported .NET Versions

- .NET 7 (not tested)
- .NET 6.0
- .NET 5.0 (not tested)
- .NET Core 3.1
  
### Installation

    Install-Package OpenDDD.NET

### Examples

One of the companies that are using this framework to build their software product is [Identity & Access Cloud](https://www.identityaccess.cloud). You can check out their IAM product [source code](...) for a full example of a product implemented using OpenDDD .NET.

You can also choose to jump right in with one of the templates we provide for the different supported versions of .NET. As per convention, they implement a minimal example of a [WeatherForecast](...) project.

If you're simply looking for some code snippets, see below for some example extracted from the IAM project mentioned above:

#### Env file

```bash
# Logging
CFG_LOGGING_LEVEL_DOTNET=Information
CFG_LOGGING_LEVEL=Debug

# General
CFG_GENERAL_CONTEXT=MyContext

# Auth
CFG_AUTH_ENABLED=true
CFG_AUTH_RBAC_PROVIDER=PowerIAM
CFG_AUTH_RBAC_EXTERNAL_REALM_ID=
CFG_AUTH_JWT_TOKEN_PRIVATE_KEY=arg8WLiCr3Y5tLXHHP01fh53bgTHnof8
CFG_AUTH_JWT_TOKEN_NAME=Authorization
CFG_AUTH_JWT_TOKEN_LOCATION=header
CFG_AUTH_JWT_TOKEN_SCHEME=Bearer

# Http Adapter
CFG_HTTP_URLS=https://api.myapp.com
CFG_HTTP_CORS_ALLOWED_ORIGINS=https://www.myapp.com:443,http://www.myapp.com:80
CFG_HTTP_DOCS_MAJOR_VERSIONS=2
CFG_HTTP_DOCS_DEFINITIONS=
CFG_HTTP_DOCS_ENABLED=true
CFG_HTTP_DOCS_HTTP_ENABLED=false
CFG_HTTP_DOCS_HTTPS_ENABLED=true
CFG_HTTP_DOCS_HOSTNAME=https://api.myapp.com/docs
CFG_HTTP_DOCS_AUTH_EXTRA_TOKENS=
CFG_HTTP_DOCS_TITLE="My App API"

# Persistence
CFG_PERSISTENCE_PROVIDER=Postgres
CFG_PERSISTENCE_POOLING_ENABLED=true
CFG_PERSISTENCE_POOLING_MIN_SIZE=0
CFG_PERSISTENCE_POOLING_MAX_SIZE=100

# Postgres
CFG_POSTGRES_CONN_STR="Host=postgres.myapp.com:5432;Username=some-username;Password=some-password;Database=myapp"

# PubSub
CFG_PUBSUB_PROVIDER=Rabbit
CFG_PUBSUB_MAX_DELIVERY_RETRIES=3
CFG_PUBSUB_PUBLISHER_ENABLED=true

# Monitoring
CFG_MONITORING_PROVIDER=AppInsights

# PowerIAM
CFG_POWERIAM_URL=https://api.poweriam.com/mycompany/myapp

# Service Bus
CFG_SERVICEBUS_CONN_STR=
CFG_SERVICEBUS_SUB_NAME=

# Rabbit
CFG_RABBIT_HOST=rabbit.myapp.com
CFG_RABBIT_PORT=5672
CFG_RABBIT_USERNAME=some-username
CFG_RABBIT_PASSWORD=some-password

# Email
CFG_EMAIL_ENABLED=true
CFG_EMAIL_PROVIDER=smtp
CFG_EMAIL_SMTP_HOST=some.host.com
CFG_EMAIL_SMTP_PORT=1025
CFG_EMAIL_SMTP_USERNAME=some-username
CFG_EMAIL_SMTP_PASSWORD=some-password
```

#### Program.cs

```c#
namespace Main
{
    public class Program
    {
        public static void Main(string[] args)
            => CreateWebHostBuilder(args).Build().Run();
        
        public static IWebHostBuilder CreateWebHostBuilder(string[] args) =>
            WebHost.CreateDefaultBuilder(args)
                .UseKestrel()
                .UseStartup<Startup>()
                .AddEnvFile("ENV_FILE", "CFG_")
                .AddSettings()
                .AddCustomSettings()
                .AddLogging();
    }
}
```

#### Startup.cs

```c#
namespace Main
{
    public class Startup
    {
        # ...
        
        public void ConfigureServices(IServiceCollection services)
        {
            // DDD.NETCore
            services.AddAccessControl(_settings);
            services.AddMonitoring(_settings);
            services.AddPersistence(_settings);
            services.AddPubSub(_settings);
            services.AddSerialization(_settings);

            // App
            AddDomainServices(services);
            AddApplicationService(services);
            AddSecondaryAdapters(services);
            AddPrimaryAdapters(services);
            AddHooks(services);
        }

        public void Configure(
            IApplicationBuilder app, 
            IWebHostEnvironment env,
            IApplicationLifetime lifetime)
        {
            // DDD.NETCore
            app.AddAccessControl(_settings);
            app.AddHttpAdapter(_settings);
            app.AddControl(lifetime);
        }

        # ...
    }
}
```

#### CreateAccountAction.cs

```c#
namespace Application.Actions
{
    public class CreateAccountAction : Action<CreateAccountCommand, User>
    {
        public CreateAccountAction(ActionDependencies deps) : base(deps)
        {
            
        }

        public override async Task<User> ExecuteAsync(
            CreateAccountCommand command,
            ActionId actionId,
            CancellationToken ct)
        {
            // Run
            var existing =
                await _userRepository.GetWithEmailAsync(
                    command.Email,
                    actionId,
                    ct);

            if (existing != null)
                throw DomainException.AlreadyExists("user", "email", command.Email);

            if (command.Password != command.RepeatPassword)
                throw DomainException.InvariantViolation("The passwords don't match.");

            var user =
                User.Create(
                    userId: UserId.Create(await _userRepository.GetNextIdentityAsync()),
                    firstName: command.FirstName,
                    lastName: command.LastName,
                    email: command.Email,
                    isSuperUser: false,
                    actionId: actionId,
                    ct: ct);

            user.SetPassword(command.Password, actionId, ct);
            user.RequestEmailValidation(actionId, ct);
            
            // Persist
            await _userRepository.SaveAsync(user, actionId, ct);
            
            // Publish
            await _domainPublisher.PublishAsync(new AccountCreated(user, actionId));
            
            // Return
            return user;
        }
    }
}
```

#### User.cs

```c#
namespace Domain.Model.User
{
    public class User : Aggregate, IAggregate, IEquatable<User>
    {
        public UserId UserId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public Email Email { get; set; }
        public DateTime? EmailVerifiedAt { get; set; }
        
        # ...

        public static User Create(
            UserId userId,
            string firstName,
            string lastName,
            Email email,
            bool isSuperUser,
            ActionId actionId,
            CancellationToken ct)
        {
            var user =
                new User
                {
                    DomainModelVersion = IamDomainModelVersion.Latest(),
                    UserId = userId,
                    FirstName = firstName,
                    LastName = lastName,
                    Email = email,
                    EmailVerifiedAt = null,
                    EmailVerificationRequestedAt = null,
                    EmailVerificationCodeCreatedAt = null,
                    EmailVerificationCode = null,
                    IsSuperUser = isSuperUser,
                    RealmIds = new List<RealmId>()
                };
            
            user.SetPassword(Password.Generate(), actionId, ct);

            user.Validate();

            return user;
        }

        public bool IsEmailVerified()
            => EmailVerifiedAt != null;
        
        public bool IsEmailVerificationRequested()
            => EmailVerificationRequestedAt != null;
        
        public bool IsEmailVerificationCodeExpired()
            => DateTime.UtcNow.Subtract(EmailVerificationCodeCreatedAt!.Value).TotalSeconds >= (60 * 30);
        
        public async Task SendEmailVerificationEmailAsync(Uri verifyEmailUrl, IEmailPort emailAdapter, ActionId actionId, CancellationToken ct)
        {
            if (Email == null)
                throw DomainException.InvariantViolation("The user has no email.");
            
            if (IsEmailVerified())
                throw DomainException.InvariantViolation("The email is already verified.");
            
            if (!IsEmailVerificationRequested())
                throw DomainException.InvariantViolation("Email verification hasn't been requested.");

            # ...
        }
        
        # ...
    }
}
```

### Documentation:
  
Documentation is coming in the release of v1.0.0-rc.1.

### Semantic versioning

We have chosen the SemVer2.0 policy for versioning of the http api through the primary http adapters.

In SemVer2.0, backwards *compatible* changes increments the patch- and minor versions, whereas backwards *incompatible* changes increments the major version.

See the table below for examples of when to increment which version.

| Code Status                               | Stage         | Rule                                                               | Example Version |
|-------------------------------------------|---------------|--------------------------------------------------------------------|-----------------|
| First release                             | New product   | Start with 1.0.0                                                   | 1.0.0           |
| Backward-compatible bug fixes             | Patch release | Increment the third digit                                          | 1.0.1           |
| Backward compatible new features          | Minor release | Increment the middle digit and reset last digit to zero            | 1.1.0           |
| Changes that break backward compatibility | Major release | Increment the first digit and reset middle and last digits to zero | 2.0.0           |

### Contribution:
  
If you want to contribute to the code base, create a pull request on the develop branch. Feel very free to reach out to us by email or via social media.

### Roadmap v1.0.0:

- [ ] GitHub README
- [ ] NuGet README
- [ ] Full Sample Project
- [ ] Quickstart Guide
- [ ] Full Test Coverage
- [ ] Visual Studio Project Template .NET 7
- [x] Visual Studio Project Template .NET 6.0
- [ ] Visual Studio Project Template .NET 5.0
- [ ] Visual Studio Project Template .NET Core 3.1
- [x] Start Context
- [x] Stop Context
- [x] Control
- [x] On-the-fly aggregate migration
- [x] Auto-code Generation from domain.yml File
- [x] Postgres Dead Letter Queue
- [x] Memory Dead Letter Queue
- [x] Dead Letter Queue
- [x] Handle Poisonous Messages
- [x] Re-publish Failed Events
- [x] Postgres Outbox
- [x] Memory Outbox
- [x] Outbox
- [x] Domain Event Publishing
- [x] Integration Event Publishing
- [x] Rabbit Event Adapter
- [x] Memory Event Adapter
- [x] PubSub
- [x] Auth Domain Service
- [x] Auth
- [x] Aggregate
- [x] Entity
- [x] Value Object
- [x] Domain Event
- [x] Integration Event
- [x] Repository
- [x] Building Blocks
- [x] Domain Service
- [x] Infrastructure Service
- [x] Application Service
- [x] Auto-Generated Swagger HTTP Adapter Documentation
- [x] HTTP Adapter
- [x] Email Adapter
- [x] Persistence Service
- [x] Postgres Repository
- [x] Memory Repository

### Roadmap Future:

- [ ] Monitoring
- [ ] Tasks support
- [ ] Migration of all aggregates not up-to-date
- [ ] Periodic Jobs
- [ ] Interval Jobs
- [ ] One-off Jobs
- [ ] Command Line Interface (CLI)
- [ ] CLI Operation: Migrate
- [ ] CLI Operation: Create Migration
- [ ] CLI Operation: Increment Domain Model Version
- [ ] Admin Dashboard
- [ ] Admin Tool: Inspect Dead Letter Queue
- [ ] Admin Tool: Republish Dead Letters
- [ ] Administration

### Release Notes:

**1.0.0-alpha.10** - 2023-04-xx

- Add more synchronous versions of methods used by tests.

**1.0.0-alpha.9** - 2023-04-19

- Add synchronous versions of methods. (**breaking**)

**1.0.0-alpha.8** - 2023-04-11

- Add support for context hooks.
- Add error codes support. (**breaking**)
- Fix database connections leak.
- Add support for enabling/disabling publishers in tests.
- Add assertion methods.
- Fix issues with running tests in parallell.
- Use newtonsoft json everywhere. (**breaking**)
- Add base email adapter. (**breaking**)
- Properly start & stop outbox. (**breaking**)
- Properly start & stop repositories. (**breaking**)

**1.0.0-alpha.7** - 2023-01-01

- Add credentials support to smtp adapter.
- Use api version 2.0.0 in poweriam adapter.

**1.0.0-alpha.6** - 2023-01-01

- Add base class for domain services.
- Use new permissions string format: "\<domain\>:\<permission\>". (**breaking**)

**1.0.0-alpha.5** - 2022-12-26

- Refactor to follow semver2.0 strictly in http adapter. (**breaking**)
- Add support for configuring persistence pooling.
- Add html support to email port. (**breaking**)
- Fix memory leak where db connections weren't closed.

**1.0.0-alpha.4** - 2022-12-10

- Add configuration setting for which server urls to listen to. (**breaking**)
- Fix concurrency issues with memory repositories.
- Add support for IAM ports.
- Add 'PowerIAM' adapter.
- Add RBAC auth settings. (**breaking**)
- Add a base 'Migrator' class. (**breaking**)

**1.0.0-alpha.3** - 2022-11-20

- Refactor JwtToken and add IdToken. (**breaking**)
- Add more tasks to code generation tool.
- Add support for http put methods to code generation tool.
- Add some missing repository method implementations.
- Add GetAsync(IEnumerable<...> ...) to repositories.
- Add convenience methods to ApplicationExtensions.
- Return 400 http status code on domain- and invariant exceptions in primary http adapter.

**1.0.0-alpha.2** - 2022-10-09

- Make the hexagonal architecture more represented in the namespaces.
 
**1.0.0-alpha.1** - 2022-10-02

This is the first (alpha) release of the framework.
Please try it out and submit tickets or otherwise reach out if you find any issues or have any questions.

**0.9.0-alpha7** - 2022-07-31

First alpha release on nuget.org.
