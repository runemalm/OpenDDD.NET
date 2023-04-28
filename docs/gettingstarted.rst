############
Installation
############

Install the framework using the package manager or the `dotnet CLI <https://learn.microsoft.com/en-us/nuget/consume-packages/install-use-packages-dotnet-cli>`_::

    $ dotnet add package OpenDDD.NET


###################
Example Application
###################

The quickest way to get a taste of this framework is to check out the code snippets on the :doc:`start page<index>`.

The reommended and easy way to get started quickly is to use the WeatherForecast `project templates <https://todo>`_.

Next, we'll guide you through the building blocks to get you started.


###############
Building Blocks
###############

We will now cover all the ``building blocks`` of the framework:

* :ref:`Env files`
* :ref:`Domain Model Version`
* :ref:`Program.cs`
* :ref:`Startup.cs`
* :ref:`Commands`
* :ref:`Actions`
* :ref:`Entities`
* :ref:`Repositories`
* :ref:`Events`
* :ref:`Listeners`
* :ref:`Domain Services`
* :ref:`Errors`
* :ref:`Converters`
* :ref:`Migrators`
* :ref:`Unit Tests`


Env files
---------

An `env file <https://12factor.net/config>`_ is used to configure your bounded context for a specific environment.

You will have one env file for each of your environments:

- env.prod
- env.staging
- env.local
- env.test

.. tip:: Copy one of the env.sample files to quickly create an env file with sample content.

Set the ``ENV_FILE`` environment variable to specify the name of the env file or the actual contents of it. This is how you define the configuration to use.

If you load this variable with a filename, the framwork will look for an env file with that name in the current directory, or any of the parent directories. If you on the other hand specify the actual contents of the env file in this variable, remember to first serialize it into a json string. The framework is smart enough to detect if the ``ENV_FILE`` variable value is a filename or a json encoded string with it's contents.

.. note:: The example env file below is not suitable for production. It has authentication disabled and uses memory implementation of adapters to get you started quickly.

Example env file::

    # Logging
    CFG_LOGGING_LEVEL_DOTNET=Information
    CFG_LOGGING_LEVEL=Debug

    # General
    CFG_GENERAL_CONTEXT=Weather

    # Auth
    CFG_AUTH_ENABLED=false
    CFG_AUTH_RBAC_PROVIDER=
    CFG_AUTH_RBAC_EXTERNAL_REALM_ID=
    CFG_AUTH_JWT_TOKEN_PRIVATE_KEY=
    CFG_AUTH_JWT_TOKEN_NAME=
    CFG_AUTH_JWT_TOKEN_LOCATION=
    CFG_AUTH_JWT_TOKEN_SCHEME=

    # Http Adapter
    CFG_HTTP_URLS=http://localhost:5051
    CFG_HTTP_CORS_ALLOWED_ORIGINS=http://localhost:5051
    CFG_HTTP_DOCS_MAJOR_VERSIONS=1
    CFG_HTTP_DOCS_DEFINITIONS=
    CFG_HTTP_DOCS_ENABLED=true
    CFG_HTTP_DOCS_HTTP_ENABLED=true
    CFG_HTTP_DOCS_HTTPS_ENABLED=false
    CFG_HTTP_DOCS_HOSTNAME=localhost:5051
    CFG_HTTP_DOCS_HTTP_PORT=80
    CFG_HTTP_DOCS_HTTPS_PORT=443
    CFG_HTTP_DOCS_AUTH_EXTRA_TOKENS=
    CFG_HTTP_DOCS_TITLE=Weather Context API

    # Persistence
    CFG_PERSISTENCE_PROVIDER=Memory
    CFG_PERSISTENCE_POOLING_ENABLED=true
    CFG_PERSISTENCE_POOLING_MIN_SIZE=0
    CFG_PERSISTENCE_POOLING_MAX_SIZE=100

    # Postgres
    CFG_POSTGRES_CONN_STR=

    # PubSub
    CFG_PUBSUB_PROVIDER=Memory
    CFG_PUBSUB_MAX_DELIVERY_RETRIES=3
    CFG_PUBSUB_PUBLISHER_ENABLED=true

    # Monitoring
    CFG_MONITORING_PROVIDER=Memory

    # Rabbit
    CFG_RABBIT_HOST=
    CFG_RABBIT_PORT=
    CFG_RABBIT_USERNAME=
    CFG_RABBIT_PASSWORD=

    # Email
    CFG_EMAIL_ENABLED=true
    CFG_EMAIL_PROVIDER=smtp
    CFG_EMAIL_SMTP_HOST=localhost
    CFG_EMAIL_SMTP_PORT=1025
    CFG_EMAIL_SMTP_USERNAME=
    CFG_EMAIL_SMTP_PASSWORD=


Domain Model Version
--------------------

Since this framework is all about focusing on an evolving and up-to-date domain model, we need to have a representation of a domain model version.

Create this class by subclassing the ``DomainModelVersion`` base class.

As your model evolves, you will increment the ``LatestString`` and add appropriate migration methods to the entity migrators. More on :ref:`migrators in a later section <Migrators>`.

Example domain model version::

    namespace Domain.Model
    {
        public class DomainModelVersion : DDD.Domain.Model.DomainModelVersion
        {
            public const string LatestString = "1.0.0";
            
            public DomainModelVersion(string dotString) : base(dotString) { }

            public static DomainModelVersion Latest()
            {
                return new DomainModelVersion(LatestString);
            }
        }
    }


Program.cs
----------

Use the ``AddXxx()`` extension methods of the framework to properly configure the .NET host and application.

Example Program.cs file::

    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;
    using OpenDDD.NET.Extensions;
    using Main.Extensions;

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


Startup.cs
----------

Since part of the design philosophy behind this framwork is to follow the hexagonal architecture, and to make this intent clear through the structure of the code, the ``Startup.cs`` file is written according to a specific convention.

See the example below and create your Startup.cs file.

Example Startup.cs file::

    using System.Reflection;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using DDD.Application.Settings;
    using DDD.Application.Settings.Persistence;
    using OpenDDD.NET.Extensions;
    using OpenDDD.NET.Hooks;
    using Main.Extensions;
    using Main.NETCore.Hooks;
    using Application.Actions;
    using Application.Actions.Commands;
    using Domain.Model.Forecast;
    using Domain.Model.Summary;
    using Infrastructure.Ports.Adapters.Domain;
    using Infrastructure.Ports.Adapters.Http.v1;
    using Infrastructure.Ports.Adapters.Interchange.Translation;
    using Infrastructure.Ports.Adapters.Repositories.Memory;
    using Infrastructure.Ports.Adapters.Repositories.Migration;
    using Infrastructure.Ports.Adapters.Repositories.Postgres;
    using HttpCommonTranslation = Infrastructure.Ports.Adapters.Http.Common.Translation;

    namespace Main
    {
        public class Startup
        {
            private ISettings _settings;

            public Startup(
                ISettings settings)
            {
                _settings = settings;
            }
            
            public void ConfigureServices(IServiceCollection services)
            {
                // OpenDDD.NET
                services.AddAccessControl(_settings);
                services.AddMonitoring(_settings);
                services.AddPersistence(_settings);
                services.AddPubSub(_settings);
                services.AddTransactional(_settings);

                // App
                AddDomainServices(services);
                AddApplicationService(services);
                AddSecondaryAdapters(services);
                AddPrimaryAdapters(services);
                AddSerialization(services);
                AddHooks(services);
            }

            public void Configure(
                IApplicationBuilder app, 
                IWebHostEnvironment env,
                IHostApplicationLifetime lifetime)
            {
                // OpenDDD.NET
                app.AddAccessControl(_settings);
                app.AddHttpAdapter(_settings);
                app.AddControl(lifetime);
            }

            // App
            
            private void AddDomainServices(IServiceCollection services)
            {
                services.AddTransient<IForecastDomainService, ForecastDomainService>();
            }

            private void AddApplicationService(IServiceCollection services)
            {
                AddActions(services);
            }
            
            private void AddSecondaryAdapters(IServiceCollection services)
            {
                services.AddEmailAdapter(_settings);
                AddRepositories(services);
            }

            private void AddPrimaryAdapters(IServiceCollection services)
            {
                AddHttpAdapters(services);
                AddInterchangeEventAdapters(services);
                AddDomainEventAdapters(services);
            }

            private void AddHooks(IServiceCollection services)
            {
                services.AddTransient<IOnBeforePrimaryAdaptersStartedHook, OnBeforePrimaryAdaptersStartedHook>();
            }

            private void AddSerialization(IServiceCollection services)
            {
                services.AddSerialization(_settings);
            }

            private void AddActions(IServiceCollection services)
            {
                services.AddAction<GetAverageTemperatureAction, GetAverageTemperatureCommand>();
                services.AddAction<NotifyWeatherPredictedAction, NotifyWeatherPredictedCommand>();
                services.AddAction<PredictWeatherAction, PredictWeatherCommand>();
            }

            private void AddHttpAdapters(IServiceCollection services)
            {
                var mvcCoreBuilder = services.AddHttpAdapter(_settings);
                AddHttpAdapterCommon(services);
                AddHttpAdapterV1(services, mvcCoreBuilder);
            }

            private void AddHttpAdapterV1(IServiceCollection services, IMvcCoreBuilder mvcCoreBuilder)
            {
                mvcCoreBuilder.AddApplicationPart(Assembly.GetAssembly(typeof(HttpAdapter)));
                services.AddTransient<HttpCommonTranslation.Commands.PredictWeatherCommandTranslator>();
                services.AddTransient<HttpCommonTranslation.ForecastIdTranslator>();
                services.AddTransient<HttpCommonTranslation.ForecastTranslator>();
                services.AddTransient<HttpCommonTranslation.SummaryIdTranslator>();
                services.AddTransient<HttpCommonTranslation.SummaryTranslator>();
            }
            
            private void AddHttpAdapterCommon(IServiceCollection services)
            {
                services.AddHttpCommandTranslator<HttpCommonTranslation.Commands.PredictWeatherCommandTranslator>();

                services.AddHttpBuildingBlockTranslator<HttpCommonTranslation.ForecastIdTranslator>();
                services.AddHttpBuildingBlockTranslator<HttpCommonTranslation.ForecastTranslator>();
                services.AddHttpBuildingBlockTranslator<HttpCommonTranslation.SummaryIdTranslator>();
                services.AddHttpBuildingBlockTranslator<HttpCommonTranslation.SummaryTranslator>();
            }
            
            private void AddInterchangeEventAdapters(IServiceCollection services)
            {
                services.AddTransient<IIcForecastTranslator, IcForecastTranslator>();
            }
            
            private void AddDomainEventAdapters(IServiceCollection services)
            {
                services.AddListener<WeatherPredictedListener>();
            }
            
            private void AddRepositories(IServiceCollection services)
            {
                if (_settings.Persistence.Provider == PersistenceProvider.Memory)
                {
                    services.AddRepository<IForecastRepository, MemoryForecastRepository>();
                    services.AddRepository<ISummaryRepository, MemorySummaryRepository>();
                }
                else if (_settings.Persistence.Provider == PersistenceProvider.Postgres)
                {
                    services.AddRepository<IForecastRepository, PostgresForecastRepository>();
                    services.AddRepository<ISummaryRepository, PostgresSummaryRepository>();
                }
                services.AddMigrator<ForecastMigrator>();
                services.AddMigrator<SummaryMigrator>();
            }
        }
    }


Commands
--------

All command classes need to subclass the ``Command`` class.

The command class is basically a data transfer object (DTO), except of course it has a very specific meaning in terms of your domain model.

The command is passed to the relevant action when an actor requests it.

Example command::

    using System.Collections.Generic;
    using System.Linq;
    using DDD.Application;
    using DDD.Application.Error;
    using DDD.Domain.Model.Validation;
    using Domain.Model.User;

    namespace Application.Actions.Commands
    {
        public class CreateAccountCommand : Command
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public Email Email { get; set; }
            public string Password { get; set; }
            public string RepeatPassword { get; set; }

            public override void Validate()
            {
                var errors = GetErrors();

                if (errors.Any())
                    throw new InvalidCommandException(this, errors);
            }

            public override IEnumerable<ValidationError> GetErrors()
            {
                var errors = new Validator<CreateAccountCommand>(this)
                    .NotNullOrEmpty(command => command.FirstName)
                    .NotNullOrEmpty(command => command.LastName)
                    .Email(command => command.Email.ToString())
                    .NotNullOrEmpty(command => command.Password.ToString())
                    .NotNullOrEmpty(command => command.RepeatPassword.ToString())
                    .Errors();

                return errors;
            }
        }
    }


Actions
-------

All action classes need to subclass the ``Action<TCommand, TReturn>`` class.

The ``ExecuteAsync()`` method is where you fetch your aggregate roots and delegate domain logic to them and/or domain services.

If your aggregate roots or domain services need to publish events or use any adapter, you inject them via the constructor and pass along in the calls that drive your domain logic through these objects.

Remember that an aggregate is only allowed to change the state of a single aggregate root at a time. It must also delegate all domain logic to the aggregate roots and/or domain services. Domain logic doesn't belong in the application layer.

.. warning:: Delegate all domain logic to aggregate roots or domain services.

.. warning:: Only act upon one aggregate root per action.

You register your action classes with the DI container like this::

    services.AddAction<CreateAccountAction, CreateAccountCommand>();

Example action::

    using System.Threading;
    using System.Threading.Tasks;
    using DDD.Application;
    using DDD.Domain.Model.Error;
    using DDD.Domain.Services.Auth;
    using DDD.Infrastructure.Ports.PubSub;
    using DDD.Infrastructure.Services.Persistence;
    using Application.Actions.Commands;
    using Domain.Model.User;

    namespace Application.Actions
    {
        public class CreateAccountAction : DDD.Application.Action<CreateAccountCommand, User>
        {
            private readonly IUserRepository _userRepository;
            
            public CreateAccountAction(
                IAuthDomainService authDomainService,
                IUserRepository userRepository,
                IDomainPublisher domainPublisher,
                IInterchangePublisher interchangePublisher,
                IOutbox outbox,
                IPersistenceService persistenceService)
                : base(authDomainService, domainPublisher, interchangePublisher, outbox, persistenceService)
            {
                _userRepository = userRepository;
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


Entities
--------

The entities subclass either the ``Aggregate`` class if it's an aggregate root, or the ``Entity`` class otherwise.

They need to implement the ``IEquatable<>`` interface, so that assertions in the unit tests can compare them to each other.

Actions use the methods of aggregate roots to drive the domain logic, passing adapters and publishers needed as arguments.

Example aggregate root::

    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.WebUtilities;
    using DDD.Application;
    using DDD.Domain.Model.BuildingBlocks.Aggregate;
    using DDD.Domain.Model.BuildingBlocks.Entity;
    using DDD.Domain.Model.Error;
    using DDD.Domain.Model.Validation;
    using DDD.Infrastructure.Ports.Email;
    using Domain.Model.Realm;
    using ContextDomainModelVersion = Domain.Model.DomainModelVersion;
    using SaltClass = Domain.Model.User.Salt;

    namespace Domain.Model.User
    {
        public class User : Aggregate, IAggregate, IEquatable<User>
        {
            public UserId UserId { get; set; }
            EntityId IAggregate.Id => UserId;
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public Email Email { get; set; }
            public DateTime? EmailVerifiedAt { get; set; }
            public DateTime? EmailVerificationRequestedAt { get; set; }
            public DateTime? EmailVerificationCodeCreatedAt { get; set; }
            public EmailVerificationCode? EmailVerificationCode { get; set; }
            public Password Password { get; set; }
            public Salt Salt { get; set; }
            public string ResetPasswordCode { get; set; }
            public DateTime? ResetPasswordCodeCreatedAt { get; set; }
            public bool IsSuperUser { get; set; }
            public ICollection<RealmId> RealmIds { get; set; }

            public User() {}

            // Public
            
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
                        DomainModelVersion = ContextDomainModelVersion.Latest(),
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

                // Re-generate code
                if (EmailVerificationCode != null)
                    RegenerateEmailVerificationCode();

                var link = $"{verifyEmailUrl}?code={EmailVerificationCode}&userId={UserId}";

                await emailAdapter.SendAsync(
                    "no-reply@poweriam.com", 
                    "PowerIAM", 
                    Email.Value,
                    $"{FirstName} {LastName}",
                    $"Verify your email", 
                    $"Hi, please verify this email address belongs to you by clicking the link: <a href=\"{link}\">Verify Your Email</a>",
                    true,
                    ct);
            }
            
            public async Task VerifyEmail(EmailVerificationCode code, ActionId actionId, CancellationToken ct)
            {
                if (Email == null)
                    throw VerifyEmailException.UserHasNoEmail();
                
                if (IsEmailVerified())
                    throw VerifyEmailException.AlreadyVerified();

                if (!IsEmailVerificationRequested())
                    throw VerifyEmailException.NotRequested();

                if (!code.Equals(EmailVerificationCode))
                    throw VerifyEmailException.InvalidCode();
                    
                if (IsEmailVerificationCodeExpired())
                    throw VerifyEmailException.CodeExpired();

                EmailVerifiedAt = DateTime.UtcNow;
                EmailVerificationRequestedAt = null;
                EmailVerificationCode = null;
                EmailVerificationCodeCreatedAt = null;
            }

            public void AddToRealm(RealmId realmId, ActionId actionId)
            {
                if (IsInRealm(realmId))
                    throw DomainException.InvariantViolation($"User {UserId} already belongs to realm {realmId}.");
                
                RealmIds.Add(realmId);
            }
            
            public async Task ForgetPasswordAsync(Uri resetPasswordUri, IEmailPort emailAdapter, ActionId actionId, CancellationToken ct)
            {
                if (Email == null)
                    throw DomainException.InvariantViolation("Can't send reset password email, the user has no email.");

                ResetPasswordCode = Guid.NewGuid().ToString("n").Substring(0, 24);
                ResetPasswordCodeCreatedAt = DateTime.UtcNow;

                resetPasswordUri = new Uri(QueryHelpers.AddQueryString(resetPasswordUri.ToString(), "code", ResetPasswordCode));
                
                var link = resetPasswordUri.ToString();

                await emailAdapter.SendAsync(
                    "no-reply@poweriam.com", 
                    "PowerIAM", 
                    Email.Value, 
                    $"{FirstName} {LastName}",
                    $"Your reset password link", 
                    $"Hi, someone said you forgot your password. If this wasn't you then ignore this email.<br>" +
                    $"Follow the link to set your new password: <a href=\"{link}\">Reset Your Password</a>",
                    true,
                    ct);
            }
            
            public bool IsInRealm(RealmId realmId)
                => RealmIds.Contains(realmId);
            
            public bool IsValidPassword(string password)
                => Salt != null && Password != null && (Password.CreateAndHash(password, Salt) == Password);
            
            public void RemoveFromRealm(RealmId realmId, ActionId actionId)
            {
                if (!IsInRealm(realmId))
                    throw DomainException.InvariantViolation($"User {UserId} doesn't belong to realm {realmId}.");
                
                RealmIds.Remove(realmId);
            }
            
            public async Task ResetPassword(string newPassword, ActionId actionId, CancellationToken ct)
            {
                if (ResetPasswordCode == null)
                    throw DomainException.InvariantViolation(
                        "Can't reset password, there's no reset password code.");
                
                if (DateTime.UtcNow.Subtract(ResetPasswordCodeCreatedAt.Value).TotalMinutes > 59)
                    throw DomainException.InvariantViolation(
                        "The reset password link has expired. Please generate a new one and try again.");
                
                SetPassword(newPassword, actionId, ct);
                
                ResetPasswordCode = null;
                ResetPasswordCodeCreatedAt = null;
            }
            
            public void SetPassword(string password, ActionId actionId, CancellationToken ct)
            {
                Salt = SaltClass.Generate();
                Password = Password.CreateAndHash(password, Salt);
            }
            
            public void RequestEmailValidation(ActionId actionId, CancellationToken ct)
            {
                EmailVerifiedAt = null;
                EmailVerificationRequestedAt = DateTime.UtcNow;
                RegenerateEmailVerificationCode();
            }

            // Private
            
            private void RegenerateEmailVerificationCode()
            {
                EmailVerificationCode = EmailVerificationCode.Generate();
                EmailVerificationCodeCreatedAt = DateTime.UtcNow;
            }

            protected void Validate()
            {
                var validator = new Validator<User>(this);

                var errors = validator
                    .NotNull(bb => bb.UserId.Value)
                    .NotNullOrEmpty(bb => bb.FirstName)
                    .NotNullOrEmpty(bb => bb.LastName)
                    .NotNullOrEmpty(bb => bb.Email.Value)
                    .Errors()
                    .ToList();

                if (errors.Any())
                {
                    throw DomainException.InvariantViolation(
                        $"User is invalid with errors: " +
                        $"{string.Join(", ", errors.Select(e => $"{e.Key} {e.Details}"))}");
                }
            }

            // Equality

            public bool Equals(User? other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return base.Equals(other) && UserId.Equals(other.UserId) && FirstName == other.FirstName && LastName == other.LastName && Email.Equals(other.Email) && Nullable.Equals(EmailVerifiedAt, other.EmailVerifiedAt) && Nullable.Equals(EmailVerificationRequestedAt, other.EmailVerificationRequestedAt) && Nullable.Equals(EmailVerificationCodeCreatedAt, other.EmailVerificationCodeCreatedAt) && Equals(EmailVerificationCode, other.EmailVerificationCode) && Password.Equals(other.Password) && Salt.Equals(other.Salt) && ResetPasswordCode == other.ResetPasswordCode && Nullable.Equals(ResetPasswordCodeCreatedAt, other.ResetPasswordCodeCreatedAt) && IsSuperUser == other.IsSuperUser && RealmIds.Equals(other.RealmIds);
            }

            public override bool Equals(object? obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((User)obj);
            }

            public override int GetHashCode()
            {
                var hashCode = new HashCode();
                hashCode.Add(base.GetHashCode());
                hashCode.Add(UserId);
                hashCode.Add(FirstName);
                hashCode.Add(LastName);
                hashCode.Add(Email);
                hashCode.Add(EmailVerifiedAt);
                hashCode.Add(EmailVerificationRequestedAt);
                hashCode.Add(EmailVerificationCodeCreatedAt);
                hashCode.Add(EmailVerificationCode);
                hashCode.Add(Password);
                hashCode.Add(Salt);
                hashCode.Add(ResetPasswordCode);
                hashCode.Add(ResetPasswordCodeCreatedAt);
                hashCode.Add(IsSuperUser);
                hashCode.Add(RealmIds);
                return hashCode.ToHashCode();
            }
        }
    }


Repositories
------------

A repository is the interface for getting & saving your aggregate root from/to the database.

Subclass the ``Repository`` base class for each aggregate root.

There are some base methods for e.g. getting all aggregate roots, getting by ID, saving an aggregate root, etc. You will need to add methods for the queries that are specific to your aggregate root and domain model.

You will create one interface per repository, and one adapter for each of the technology implementations you want to support.

E.g. for a user repository, you might need to create the following classes:

- IUserRepository
- MemoryUserRepository
- PostgresUserRepository

Example repository::

    using System.Collections.Generic;
    using System.Threading;
    using System.Threading.Tasks;
    using DDD.Application;
    using DDD.Application.Settings;
    using DDD.Infrastructure.Ports.Adapters.Common.Translation.Converters;
    using DDD.Infrastructure.Ports.Adapters.Repository.Postgres;
    using DDD.Infrastructure.Services.Persistence;
    using Domain.Model.Realm;
    using Domain.Model.User;
    using Infrastructure.Ports.Adapters.Repository.Migration;

    namespace Infrastructure.Ports.Adapters.Repository.Postgres
    {
        public class PostgresUserRepository : PostgresRepository<User, UserId>, IUserRepository
        {
            public PostgresUserRepository(ISettings settings, UserMigrator migrator, IPersistenceService persistenceService, SerializerSettings serializerSettings) 
                : base(settings, "users", migrator, persistenceService, serializerSettings)
            {
                
            }
            
            public Task<IEnumerable<User>> GetInRealmAsync(RealmId realmId, ActionId actionId, CancellationToken ct)
                => GetWithAsync(user => user.RealmIds.Contains(realmId), actionId, ct);
            
            public Task<User?> GetWithEmailAsync(Email email, ActionId actionId, CancellationToken ct)
                => GetFirstOrDefaultWithAsync(new List<(string, object)>() { ("Email", email) }, actionId, ct);
            
            public Task<User?> GetWithEmailVerificationCodeAsync(EmailVerificationCode code, ActionId actionId, CancellationToken ct)
                => GetFirstOrDefaultWithAsync(u => u.EmailVerificationCode != null && u.EmailVerificationCode.Equals(code), actionId, ct);

            public Task<User?> GetWithResetPasswordCodeAsync(string code, ActionId actionId, CancellationToken ct)
                => GetFirstOrDefaultWithAsync(u => u.ResetPasswordCode == code, actionId, ct);
        }
    }


Events
------

There are two classes for implementing events, ``DomainEvent`` and ``IntegrationEvent``.

Subclass the appropriate one depending on the type of event you're implementing.

.. note:: Integration event names are prefixed with ``Ic`` to easily separate them from possible domain events with the same name.

Example domain event::

    using System;
    using DDD.Application;
    using DDD.Domain.Model.BuildingBlocks.Event;

    namespace Domain.Model.User
    {
        public class AccountCreated : DomainEvent, IEquatable<AccountCreated>
        {
            public UserId UserId { get; set; }
            public Email Email { get; set; }

            public AccountCreated() : base("AccountCreated", DomainModelVersion.Latest(), "IAM", ActionId.Create()) { }

            public AccountCreated(User user, ActionId actionId) 
                : base("AccountCreated", DomainModelVersion.Latest(), "IAM", actionId)
            {
                UserId = user.UserId;
                Email = user.Email;
            }

            // Equality

            public bool Equals(AccountCreated? other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return base.Equals(other) && UserId.Equals(other.UserId) && Email.Equals(other.Email);
            }

            public override bool Equals(object? obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((AccountCreated)obj);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(base.GetHashCode(), UserId, Email);
            }
        }
    }

Example integration event::

    using System;
    using DDD.Application;
    using DDD.Domain.Model.BuildingBlocks.Event;
    using ContextDomainModelVersion = Interchange.Domain.Model.DomainModelVersion;

    namespace Interchange.Domain.Model.Forecast
    {
        public class IcWeatherPredicted : IntegrationEvent, IEquatable<IcWeatherPredicted>
        {
            public string ForecastId { get; set; }
            public DateTime Date { get; set; }
            public int TemperatureC { get; set; }
            public string SummaryId { get; set; }
            
            public IcWeatherPredicted() { }

            public IcWeatherPredicted(ActionId actionId) : base("WeatherPredicted", ContextDomainModelVersion.Latest(), "Weather", actionId) { }

            public IcWeatherPredicted(IcForecast forecast, ActionId actionId) 
                : base("WeatherPredicted", ContextDomainModelVersion.Latest(), "Interchange", actionId)
            {
                ForecastId = forecast.ForecastId;
                Date = forecast.Date;
                TemperatureC = forecast.TemperatureC;
                SummaryId = forecast.SummaryId;
            }

            // Equality

            public bool Equals(IcWeatherPredicted other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return base.Equals(other) && ForecastId == other.ForecastId && Date.Equals(other.Date) && TemperatureC == other.TemperatureC && SummaryId == other.SummaryId;
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((IcWeatherPredicted)obj);
            }

            public override int GetHashCode()
            {
                return HashCode.Combine(base.GetHashCode(), ForecastId, Date, TemperatureC, SummaryId);
            }
        }
    }


Listeners
---------

A listener is used to react to domain- and integration events.

Subscribe to an event by registering the listener with the DI container::

    services.AddListener<AccountCreatedListener>();

Your listeners will basically just create a command and pass it to the action that will be run to perform the reaction necessary.

In the example below you can see how the ``AccountCreated`` event is reacted to by calling the ``SendEmailVerification`` action.

Example domain event listener::

    using Application.Actions;
    using Application.Actions.Commands;
    using DDD.Application;
    using DDD.Infrastructure.Ports.Adapters.Common.Translation.Converters;
    using DDD.Infrastructure.Ports.PubSub;
    using DDD.Logging;
    using Domain.Model.User;
    using ContextDomainModelVersion = Domain.Model.DomainModelVersion;

    namespace Infrastructure.Ports.Adapters.Domain
    {
        public class AccountCreatedListener
            : EventListener<AccountCreated, SendEmailVerificationEmailAction, SendEmailVerificationEmailCommand>
        {
            public AccountCreatedListener(
                SendEmailVerificationEmailAction action,
                IDomainEventAdapter eventAdapter,
                IOutbox outbox,
                IDeadLetterQueue deadLetterQueue,
                ILogger logger,
                SerializerSettings serializerSettings)
                : base(
                    Context.Domain,
                    "AccountCreated",
                    ContextDomainModelVersion.Latest(),
                    action,
                    eventAdapter,
                    outbox,
                    deadLetterQueue,
                    logger,
                    serializerSettings)
            {
                
            }
            
            public override SendEmailVerificationEmailCommand CreateCommand(AccountCreated theEvent)
            {
                var command =
                    new SendEmailVerificationEmailCommand
                    {
                        UserId = theEvent.UserId
                    };

                return command;
            }
        }
    }


Domain Services
---------------

All domain service classes need to subclass the ``DomainService`` class.

Example domain service::

    using System.Threading;
    using System.Threading.Tasks;
    using DDD.Application;
    using DDD.Application.Settings;
    using DDD.Domain.Model.Auth;
    using DDD.Domain.Model.Error;
    using DDD.Domain.Services;
    using DDD.Logging;
    using Domain.Model.Assignment;
    using Domain.Model.Permission;
    using Domain.Model.Realm;

    namespace Domain.Model.Role
    {
        public class RoleDomainService : DomainService, IRoleDomainService
        {
            private readonly IAssignmentDomainService _assignmentDomainService;
            private readonly IPermissionRepository _permissionRepository;
            private readonly IRealmRepository _realmRepository;
            private readonly IRoleRepository _roleRepository;

            public RoleDomainService(
                IAssignmentDomainService assignmentDomainService,
                IPermissionRepository permissionRepository,
                IRealmRepository realmRepository,
                IRoleRepository roleRepository,
                ICredentials credentials,
                ISettings settings,
                ILogger logger) 
                : base(credentials, settings, logger)
            {
                _assignmentDomainService = assignmentDomainService;
                _permissionRepository = permissionRepository;
                _realmRepository = realmRepository;
                _roleRepository = roleRepository;
            }

            public async Task<Role> AddPermissionToRoleAsync(
                RoleId roleId, PermissionId permissionId, ActionId actionId, CancellationToken ct)
            {
                var role = await _roleRepository.GetAsync(roleId, actionId, ct);
                var permission = await _permissionRepository.GetAsync(permissionId, actionId, ct);

                if (role == null)
                    throw DomainException.NotFound("role", roleId.ToString());

                if (permission == null)
                    throw DomainException.NotFound("permission", permissionId.ToString());
                
                // Authorize
                if (role.IsInWorld())
                {
                    await _assignmentDomainService.AssurePermissionsInWorldAsync(
                        permissions: new[] { ("IAM", "ADD_PERMISSION_TO_ROLE") },
                        actionId: actionId,
                        ct: ct);
                }
                else
                {
                    await _assignmentDomainService.AssurePermissionsInRealmAsync(
                        realmId: role.RealmId.ToString(),
                        externalRealmId: "",
                        permissions: new[] { ("IAM", "ADD_PERMISSION_TO_ROLE") },
                        actionId: actionId,
                        ct: ct);
                }
                
                if (role.IsInWorld() && !permission.IsInWorld())
                    throw DomainException.InvariantViolation(
                        "Role is in world but the permission is in a realm.");
                
                if (role.IsInRealm() && !(permission.IsInRealm(role.RealmId) || permission.IsInWorld()))
                    throw DomainException.InvariantViolation(
                        "Role is in a realm but the permission is neither in that realm nor the world.");
                
                role.AddPermission(permissionId, actionId);

                return role;
            }
        }
    }

You register your domain services with the DI container like this::

    services.AddDomainService<IRoleDomainService, RoleDomainService>();


Errors
------

When an error occurs in your domain model, you manifest it by :ref:`throwing an exception <Exceptions>` containing one or more ``DomainError``.

The ``DomainError`` is of the following model:

- Code
- Message
- User Message

The ``Code`` is simply an identifier for the error.

The ``Message`` should contain a message with a description useful and aimed towards understanding the error by an integrating developer.

The ``User Message`` should contain a message with a description useful and aimed towards understanding the error in a frontend by an end user.

.. tip:: It's recommeded that the frontend development team utilizes the ``Code`` to craft the most helpful and precise user message, instead of simply relying on the more generic ``User Message``.

Example domain error::

    using DDD.Domain.Model.Error;

    namespace Domain.Model.Error
    {
        public class DomainError : DDD.Domain.Model.Error.DomainError
        {
            // Codes

            private const int VerifyEmail_NotRequested_Code = 1001;
            private const string VerifyEmail_NotRequested_Msg = "Email verification hasn't been requested.";
            private const string VerifyEmail_NotRequested_UsrMsg = "No verification of your email has been requested.";
            
            private const int VerifyEmail_AlreadyVerified_Code = 1002;
            private const string VerifyEmail_AlreadyVerified_Msg = "The email has already been verified.";
            private const string VerifyEmail_AlreadyVerified_UsrMsg = "You email address has already been verified.";

            private const int VerifyEmail_NoCode_Code = 1003;
            private const string VerifyEmail_NoCode_Msg = "The user has no email verification code.";
            private const string VerifyEmail_NoCode_UsrMsg = "An unknown error has occured. You can't verify your email because there's no email verification code.";
            
            private const int VerifyEmail_InvalidCode_Code = 1004;
            private const string VerifyEmail_InvalidCode_Msg = "The code is invalid.";
            private const string VerifyEmail_InvalidCode_UsrMsg = "The email verification code you provided is invalid. Please request a new verification code and try again.";
            
            private const int VerifyEmail_CodeExpired_Code = 1005;
            private const string VerifyEmail_CodeExpired_Msg = "The code has expired.";
            private const string VerifyEmail_CodeExpired_UsrMsg = "The verification code you provided has expired. Please request a new verification code.";
            
            private const int VerifyEmail_NoUserWithCode_Code = 1006;
            private const string VerifyEmail_NoUserWithCode_Msg = "There's no user with that code.";
            private const string VerifyEmail_NoUserWithCode_UsrMsg = "We couldn't find a user with that email verification code. Please make sure you entered the correct code and try again. Alternatively request a new verification code.";
            
            private const int VerifyEmail_UserHasNoEmail_Code = 1007;
            private const string VerifyEmail_UserHasNoEmail_Msg = "The user has no email.";
            private const string VerifyEmail_UserHasNoEmail_UsrMsg = "We couldn't verify your email because you haven't provided one. Please provide one and try verification again.";

            public static IDomainError VerifyEmail_NotRequested() => Create(VerifyEmail_NotRequested_Code, VerifyEmail_NotRequested_Msg, VerifyEmail_NotRequested_UsrMsg);
            public static IDomainError VerifyEmail_AlreadyVerified() => Create(VerifyEmail_AlreadyVerified_Code, VerifyEmail_AlreadyVerified_Msg, VerifyEmail_AlreadyVerified_UsrMsg);
            public static IDomainError VerifyEmail_NoCode() => Create(VerifyEmail_NoCode_Code, VerifyEmail_NoCode_Msg, VerifyEmail_NoCode_UsrMsg);
            public static IDomainError VerifyEmail_InvalidCode() => Create(VerifyEmail_InvalidCode_Code, VerifyEmail_InvalidCode_Msg, VerifyEmail_InvalidCode_UsrMsg);
            public static IDomainError VerifyEmail_CodeExpired() => Create(VerifyEmail_CodeExpired_Code, VerifyEmail_CodeExpired_Msg, VerifyEmail_CodeExpired_UsrMsg);
            public static IDomainError VerifyEmail_NoUserWithCode() => Create(VerifyEmail_NoUserWithCode_Code, VerifyEmail_NoUserWithCode_Msg, VerifyEmail_NoUserWithCode_UsrMsg);
            public static IDomainError VerifyEmail_UserHasNoEmail() => Create(VerifyEmail_UserHasNoEmail_Code, VerifyEmail_UserHasNoEmail_Msg, VerifyEmail_UserHasNoEmail_UsrMsg);
        }
    }

.. note:: The generic domain errors are to be found in the ``DomainError`` base class of the framework.


Exceptions
----------

The error(s) are manifested by throwing an ``DomainException``, containing the error(s).

There are two types of exceptions:

- Highly precise ``Custom exceptions`` that are specific to your domain model and
- ``Generic exceptions`` that are part of the framework and can be used by any bounded context.

It's up to you to decided which would be best to use in each of your cases.

In the example below, the ``VerifyEmailException.AlreadyVerified()`` exception is used, but it could also have been implemented using the generic ``DomainException.InvariantViolation("Email is already verified.")`` exception.

Example exception::

    using DDD.Domain.Model.Error;
    using DomainError = Domain.Model.Error.DomainError;

    namespace Domain.Model.User
    {
        public class VerifyEmailException : DomainException
        {
            public static VerifyEmailException NotRequested()
                => new VerifyEmailException(DomainError.VerifyEmail_NotRequested());
            
            public static VerifyEmailException AlreadyVerified()
                => new VerifyEmailException(DomainError.VerifyEmail_AlreadyVerified());
            
            public static VerifyEmailException NoCode()
                => new VerifyEmailException(DomainError.VerifyEmail_NoCode());
            
            public static VerifyEmailException InvalidCode()
                => new VerifyEmailException(DomainError.VerifyEmail_InvalidCode());
            
            public static VerifyEmailException CodeExpired()
                => new VerifyEmailException(DomainError.VerifyEmail_CodeExpired());
            
            public static VerifyEmailException UserHasNoEmail()
                => new VerifyEmailException(DomainError.VerifyEmail_UserHasNoEmail());
            
            public static VerifyEmailException NoUserWithCode()
                => new VerifyEmailException(DomainError.VerifyEmail_NoUserWithCode());

            public VerifyEmailException(IDomainError error) : base(error)
            {
                
            }
        }
    }

Example of throwing exceptions::

    public async Task VerifyEmail(EmailVerificationCode code, ActionId actionId, CancellationToken ct)
    {
        if (Email == null)
            throw VerifyEmailException.UserHasNoEmail();
        
        if (IsEmailVerified())
            throw VerifyEmailException.AlreadyVerified();

        if (!IsEmailVerificationRequested())
            throw VerifyEmailException.NotRequested();

        if (!code.Equals(EmailVerificationCode))
            throw VerifyEmailException.InvalidCode();
            
        if (IsEmailVerificationCodeExpired())
            throw VerifyEmailException.CodeExpired();

        EmailVerifiedAt = DateTime.UtcNow;
        EmailVerificationRequestedAt = null;
        EmailVerificationCode = null;
        EmailVerificationCodeCreatedAt = null;
    }


Converters
----------

Converters are used to transform the aggregate roots and events into strings, so that they can be persisted and/or sent on a message bus.

The OpenDDD.NET framework bases conversion on the Json.NET framework by Newtonsoft.

Json.NET comes with converters for many non-primitive generic types, such as e.g. DateTime and classes themselves. OpenDDD.NET provides missing converters for DDD-generic types such as EntityId and DomainModelVersion.

However, for ``all the types`` that are ``unique`` to your domain model, you need to create a ``corresponding converter``.

You create a converter by subclassing the ``Converter<T>`` base class.

.. note:: Don't mistake the Converter<T> class for the class with the same name in the Json.NET framework.

.. tip:: Utilize the ``ReadJsonUsingMethod()`` method of the base class to conveniently deserialize strings using your entity- and value object classes static factory methods.

Example converter::

    using System;
    using Newtonsoft.Json;
    using DDD.Infrastructure.Ports.Adapters.Common.Translation.Converters;
    using Domain.Model.User;

    namespace Infrastructure.Ports.Adapters.Common.Translation.Converters
    {
        public class EmailConverter : Converter<Email>
        {
            public override void WriteJson(
                JsonWriter writer, 
                object? value,
                JsonSerializer serializer)
            {
                writer.WriteValue(value.ToString());
            }
            
            public override object ReadJson(
                JsonReader reader, 
                Type objectType, 
                object? existingValue,
                JsonSerializer serializer)
            {
                if (reader.Value == null)
                    return null;
                return ReadJsonUsingMethod(reader, "Create", objectType);
            }
        }
    }

Registering your converter dependencies is a three-step process:

1. Create the SerializerSettings class, (if you haven't already).
2. Add the converter to the ``Converters`` collection of this class.
3. Register your SerializerSettings class with the DI container.

Example serializer settings::

    using DddSerializerSettings = DDD.Infrastructure.Ports.Adapters.Common.Translation.Converters.SerializerSettings;

    namespace Infrastructure.Ports.Adapters.Common.Translation.Converters
    {
        public class SerializerSettings : DddSerializerSettings
        {
            public SerializerSettings()
            {
                Converters.Add(new EmailConverter());
                Converters.Add(new EmailVerificationCodeConverter());
                Converters.Add(new PasswordConverter());
                Converters.Add(new SaltConverter());
            }
        }
    }

You register your serializer settings with the DI container like this::

    services.AddTransient<DddSerializerSettings, SerializerSettings>();

.. note:: The ``AddSerialization()`` call in Startup.cs of the project templates does almost all of this work for you. You just need to create your converters and add them to the collection in the constructor.


Migrators
---------

Whenver you bump your domain model version, you need to create a migration for all the entities that have changed.

Subclass the ``Migrator`` base class and implement the ``FromVX_X_X()`` method for all your entities affected by the change.

Domain model versioning is a first-class citizen in this DDD framework. Thus, migration should be as easy as possible so that the domain model can be evolved continuously with minimal effort.

.. note:: Entities will migrate on-the-fly next time they are fetched and saved by the repositories.

.. note:: If an entity has not changed it's model from one version to another, simply skip adding that method to the migrator class.

Example migrator::

    using System.Collections.Generic;
    using System.Linq;
    using DDD.Infrastructure.Ports.Adapters.Repository;
    using Domain.Model.Realm;
    using Domain.Model.User;
    using ContextDomainModelVersion = Domain.Model.DomainModelVersion;

    namespace Infrastructure.Ports.Adapters.Repository.Migration
    {
        public class UserMigrator : Migrator<User>
        {
            public UserMigrator() : base(ContextDomainModelVersion.Latest())
            {
                
            }
            
            public User FromV1_0_2(User userV1_0_2)
            {
                var salt = Salt.Generate();
                var password = Password.GenerateAndHash(salt);
                
                userV1_0_2.Salt = salt;
                userV1_0_2.Password = password;
                userV1_0_2.ResetPasswordCode = null;
                userV1_0_2.ResetPasswordCodeCreatedAt = null;
                userV1_0_2.DomainModelVersion = new ContextDomainModelVersion("1.0.3");
                return userV1_0_2;
            }

            /* There's no changes in model for v1.0.2. */

            public User FromV1_0_0(User userV1_0_0)
            {
                userV1_0_0.RealmIds = new List<RealmId>();
                userV1_0_0.IsSuperUser = false;
                userV1_0_0.DomainModelVersion = new ContextDomainModelVersion("1.0.1");
                return userV1_0_0;
            }
        }
    }

You register your migrator classes with the DI container like this::

    services.AddMigrator<UserMigrator>();


Unit Tests
----------

To achieve full test coverage of your bounded context, you need to implement a full suite of unit tests for each of your domain model actions.

.. note:: You need to create your own action unit tests base class. See the :ref:`section below <The ActionUnitTests class>` on how to do this.

Subclass ``ActionUnitTests`` for each of your action unit test suites. Then add your test methods to cover all paths.

The test methods are based on the standard ``xUnit`` testing model, so you will be familiar with the ``Arrange``, ``Act`` and ``Assert`` sections.

.. warning:: Remember that the unit tests need to reflect the domain model and ubiquitous language.

Example action unit tests::

    using Xunit;
    using Application.Actions.Commands;
    using Domain.Model.User;

    namespace Tests.Actions;

    public class VerifyEmailTests : ActionUnitTests
    {
        public VerifyEmailTests()
        {
            Configure();
            EmptyDb();
        }
        
        [Fact]
        public async Task TestSuccess_EmailVerified()
        {
            // Arrange
            await EnsureRootUserAsync();
            await EnsureIamDomainAsync();
            await EnsureIamPermissionsAsync();
            
            await CreateAccount(email: "test.testsson@poweriam.com");
            
            // Act
            var command = new VerifyEmailCommand { Code = User.EmailVerificationCode };
            await VerifyEmailAction.ExecuteAsync(command, ActionId, CancellationToken.None);
            
            await Refresh(User);
            
            // Assert
            AssertTrue(User.IsEmailVerified());
            AssertNow(User.EmailVerifiedAt);
        }
        
        [Fact]
        public async Task TestFail_UserHasNoEmail()
        {
            // Arrange
            await EnsureRootUserAsync();
            await EnsureIamDomainAsync();
            await EnsureIamPermissionsAsync();

            await CreateAccount(email: "test.testsson@poweriam.com");
            
            // ..hack
            await Refresh(User);
            User.Email = null;
            await UserRepository.SaveAsync(User, ActionId, CancellationToken.None);

            // Act & Assert
            var command = new VerifyEmailCommand()
            {
                Code = User.EmailVerificationCode
            };
            
            await AssertFailure(VerifyEmailException.UserHasNoEmail(), VerifyEmailAction.ExecuteAsync(command, ActionId, CancellationToken.None));
        }
        
        [Fact]
        public async Task TestFail_AlreadyVerified()
        {
            // Arrange
            await EnsureRootUserAsync();
            await EnsureIamDomainAsync();
            await EnsureIamPermissionsAsync();

            await CreateAccount(email: "test.testsson@poweriam.com");
            
            var command = new VerifyEmailCommand()
            {
                Code = User.EmailVerificationCode
            };

            await VerifyEmailAction.ExecuteAsync(command, ActionId, CancellationToken.None);
            
            // ..hack
            await Refresh(User);
            User.EmailVerificationCode = command.Code;
            await UserRepository.SaveAsync(User, ActionId, CancellationToken.None);

            // Act & Assert
            await AssertFailure(VerifyEmailException.AlreadyVerified(), VerifyEmailAction.ExecuteAsync(command, ActionId, CancellationToken.None));
        }
        
        [Fact]
        public async Task TestFail_NotRequested()
        {
            // Arrange
            await EnsureRootUserAsync();
            await EnsureIamDomainAsync();
            await EnsureIamPermissionsAsync();

            await CreateAccount(email: "test.testsson@poweriam.com");
            
            // ..hack
            await Refresh(User);
            User.EmailVerificationRequestedAt = null;
            await UserRepository.SaveAsync(User, ActionId, CancellationToken.None);

            // Act & Assert
            var command = new VerifyEmailCommand()
            {
                Code = User.EmailVerificationCode
            };
            
            await AssertFailure(VerifyEmailException.NotRequested(), VerifyEmailAction.ExecuteAsync(command, ActionId, CancellationToken.None));
        }
        
        [Theory]
        [InlineData(null)]
        [InlineData("some-invalid-code")]
        public async Task TestFail_InvalidCode(string? code)
        {
            // Arrange
            await EnsureRootUserAsync();
            await EnsureIamDomainAsync();
            await EnsureIamPermissionsAsync();

            await CreateAccount(email: "test.testsson@poweriam.com");

            // Act & Assert
            var command = new VerifyEmailCommand()
            {
                Code = EmailVerificationCode.Create(code)
            };
            
            await AssertFailure(VerifyEmailException.InvalidCode(), VerifyEmailAction.ExecuteAsync(command, ActionId, CancellationToken.None));
        }
        
        [Fact]
        public async Task TestFail_ExpiredCode()
        {
            // Arrange
            await EnsureRootUserAsync();
            await EnsureIamDomainAsync();
            await EnsureIamPermissionsAsync();

            await CreateAccount(email: "test.testsson@poweriam.com");

            User.EmailVerificationCodeCreatedAt = DateTime.MinValue;
            await UserRepository.SaveAsync(User, ActionId, CancellationToken.None);

            // Act & Assert
            var command = new VerifyEmailCommand()
            {
                Code = User.EmailVerificationCode
            };
            
            await AssertFailure(VerifyEmailException.CodeExpired(), VerifyEmailAction.ExecuteAsync(command, ActionId, CancellationToken.None));
        }
    }


The ActionUnitTests class
-------------------------

The purpose of your ``ActionUnitTests`` class is to provide a set of convenience methods and properties for your action unit tests to use.

The design philosophy of this framework states that the unit tests should be easy to read, understand and maintain. Furthermore they need to reflect and express the domain model in a clear manner.

To achive all of the above, your subclass will contain the following:

- Action excecution methods.
- State properties.
- ``CreateWebHostBuilder()`` (used to setup the TestServer).
- ``EmptyAggregateRepositories()`` (used to empty your repositories before each test)
- Dependency properties.
- Assertion methods.

Subclass ``ActionUnitTests`` to create your own base class for the unit tests.

.. note:: This is a very concise description of the relatively big ``ActionUnitTests`` concept. Later we'll add more documentation and guides on the topic of testing but for now you should be able to look at the example code and get started with your action testing.

Example action unit tests class::

    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Xunit;
    using OpenDDD.NET.Extensions;
    using DDD.Domain.Model.Auth;
    using DDD.Domain.Services.Auth;
    using OpenDDD.NET.Hooks;
    using Main;
    using Main.Extensions;
    using Main.NETCore.Hooks;
    using Application.Actions;
    using Application.Actions.Commands;
    using Application.Settings;
    using Domain.Model.Assignment;
    using Domain.Model.Domain;
    using Domain.Model.Permission;
    using Domain.Model.Realm;
    using Domain.Model.Role;
    using Domain.Model.User;
    using DddActionUnitTests = DDD.Tests.ActionUnitTests;

    namespace Tests
    {
        public class ActionUnitTests : DddActionUnitTests
        {
            protected global::Domain.Model.Domain.Domain Domain => Domains.First();
            protected List<global::Domain.Model.Domain.Domain> Domains = new();
            protected Permission Permission => Permissions.First();
            protected List<Permission> Permissions = new();
            protected Realm Realm => Realms.First();
            protected List<Realm> Realms = new();
            protected Role Role => Roles.First();
            protected List<Role> Roles = new();
            protected AccessToken Token;
            protected User User => Users.First();
            protected List<User> Users = new();

            // Setup

            protected override IWebHostBuilder CreateWebHostBuilder()
            {
                var builder = WebHost.CreateDefaultBuilder()
                    .UseKestrel()
                    .UseStartup<Startup>()
                    .AddEnvFile($"ENV_FILE_{ActionName}", $"CFG_{ActionName}_", "", false)
                    .AddSettings()
                    .AddCustomSettings()
                    .AddLogging();
                return builder;
            }

            protected override void EmptyAggregateRepositories(CancellationToken ct)
            {
                AssignmentRepository.DeleteAll(ActionId, CancellationToken.None);
                DomainRepository.DeleteAll(ActionId, CancellationToken.None);
                PermissionRepository.DeleteAll(ActionId, CancellationToken.None);
                RealmRepository.DeleteAll(ActionId, CancellationToken.None);
                RoleRepository.DeleteAll(ActionId, CancellationToken.None);
                UserRepository.DeleteAll(ActionId, CancellationToken.None);
            }

            protected override async Task EmptyAggregateRepositoriesAsync(CancellationToken ct)
            {
                await AssignmentRepository.DeleteAllAsync(ActionId, CancellationToken.None);
                await DomainRepository.DeleteAllAsync(ActionId, CancellationToken.None);
                await PermissionRepository.DeleteAllAsync(ActionId, CancellationToken.None);
                await RealmRepository.DeleteAllAsync(ActionId, CancellationToken.None);
                await RoleRepository.DeleteAllAsync(ActionId, CancellationToken.None);
                await UserRepository.DeleteAllAsync(ActionId, CancellationToken.None);
            }
            
            protected Task EnsureRootUserAsync()
                => new EnsureRootUser(CustomSettings, UserRepository).ExecuteAsync();
            
            protected Task EnsureIamDomainAsync()
                => new EnsureIamDomain(DomainRepository).ExecuteAsync();
            
            protected Task EnsureIamPermissionsAsync()
                => new EnsureIamPermissions(CustomSettings, UserRepository, DomainRepository, PermissionRepository).ExecuteAsync();

            // Do as actor

            protected async Task DoAsRoot(Func<Task> actionsAsync)
            {
                await AuthenticateRootUser();
                await actionsAsync();
                Credentials.JwtToken = null;
            }
            
            protected async Task DoAsUser(Func<Task> actionsAsync)
            {
                await AuthenticateUser();
                await actionsAsync();
                Credentials.JwtToken = null;
            }
        
            // Actions

            protected AddPermissionToRoleAction AddPermissionToRoleAction => TestServer.Host.Services.GetRequiredService<AddPermissionToRoleAction>();
            protected AddUserToRealmAction AddUserToRealmAction => TestServer.Host.Services.GetRequiredService<AddUserToRealmAction>();
            protected AssignRoleAction AssignRoleAction => TestServer.Host.Services.GetRequiredService<AssignRoleAction>();
            protected AuthenticateAction AuthenticateAction => TestServer.Host.Services.GetRequiredService<AuthenticateAction>();
            protected CreateAccountAction CreateAccountAction => TestServer.Host.Services.GetRequiredService<CreateAccountAction>();
            protected CreateDomainAction CreateDomainAction => TestServer.Host.Services.GetRequiredService<CreateDomainAction>();
            protected CreatePermissionAction CreatePermissionAction => TestServer.Host.Services.GetRequiredService<CreatePermissionAction>();
            protected CreateRealmAction CreateRealmAction => TestServer.Host.Services.GetRequiredService<CreateRealmAction>();
            protected CreateRoleAction CreateRoleAction => TestServer.Host.Services.GetRequiredService<CreateRoleAction>();
            protected DeleteDomainAction DeleteDomainAction => TestServer.Host.Services.GetRequiredService<DeleteDomainAction>();
            protected ForgetPasswordAction ForgetPasswordAction => TestServer.Host.Services.GetRequiredService<ForgetPasswordAction>();
            protected GetDomainsAction GetDomainsAction => TestServer.Host.Services.GetRequiredService<GetDomainsAction>();
            protected GetPermissionsGrantedAction GetPermissionsGrantedAction => TestServer.Host.Services.GetRequiredService<GetPermissionsGrantedAction>();
            protected GetRoleAssignmentsAction GetRoleAssignmentsAction => TestServer.Host.Services.GetRequiredService<GetRoleAssignmentsAction>();
            protected SendEmailVerificationEmailAction SendEmailVerificationEmailAction => TestServer.Host.Services.GetRequiredService<SendEmailVerificationEmailAction>();
            protected VerifyEmailAction VerifyEmailAction => TestServer.Host.Services.GetRequiredService<VerifyEmailAction>();

            // Auth
            
            protected IAuthDomainService AuthDomainService => TestServer.Host.Services.GetRequiredService<IAuthDomainService>();

            // Credentials
            
            protected ICredentials Credentials => TestServer.Host.Services.GetRequiredService<ICredentials>();
            
            // Settings
            
            protected ICustomSettings CustomSettings => TestServer.Host.Services.GetRequiredService<ICustomSettings>();
        
            // Domains

            protected Task<global::Domain.Model.Domain.Domain> GetIamDomainAsync() 
                => DomainRepository.GetWithNameInWorldAsync("IAM", ActionId, CancellationToken.None);
            
            // Permissions
            
            protected async Task<Permission> GetIamPermissionAsync(string name) 
                => (await PermissionRepository.GetWithNameInWorldAsync(name, (await GetIamDomainAsync()).DomainId, ActionId, CancellationToken.None))!;
            
            // Hooks
            
            protected IOnBeforePrimaryAdaptersStartedHook OnBeforePrimaryAdaptersStartedHook => TestServer.Host.Services.GetRequiredService<IOnBeforePrimaryAdaptersStartedHook>();

            // Repositories
            
            protected IAssignmentRepository AssignmentRepository => TestServer.Host.Services.GetRequiredService<IAssignmentRepository>();
            protected IDomainRepository DomainRepository => TestServer.Host.Services.GetRequiredService<IDomainRepository>();
            protected IPermissionRepository PermissionRepository => TestServer.Host.Services.GetRequiredService<IPermissionRepository>();
            protected IRealmRepository RealmRepository => TestServer.Host.Services.GetRequiredService<IRealmRepository>();
            protected IRoleRepository RoleRepository => TestServer.Host.Services.GetRequiredService<IRoleRepository>();
            protected IUserRepository UserRepository => TestServer.Host.Services.GetRequiredService<IUserRepository>();
        
            // Assertions

            protected void AssertEmailSent(Email toEmail)
                => AssertEmailSent(toEmail: toEmail, msgContains: null);

            protected void AssertEmailSent(Email toEmail, string? msgContains)
            {
                var subString = "";
                
                if (msgContains != null)
                    subString = $" containing '{msgContains}'";
                
                Assert.True(
                    EmailAdapter.HasSent(
                        toEmail: toEmail.ToString(), 
                        msgContains: msgContains),
                    $"Expected an email{subString} to be sent to {toEmail}.");
            }

            // Execute
            
            protected async Task AddPermissionToRole(PermissionId permissionId, RoleId roleId)
            {
                var command = new AddPermissionToRoleCommand
                {
                    PermissionId = permissionId,
                    RoleId = roleId
                };
            
                await AddPermissionToRoleAction.ExecuteAsync(command, ActionId, CancellationToken.None);
            }
            
            protected async Task AddUserToRealm(UserId userId, RealmId realmId)
            {
                var command = new AddUserToRealmCommand
                {
                    UserId = userId,
                    RealmId = realmId
                };
            
                await AddUserToRealmAction.ExecuteAsync(command, ActionId, CancellationToken.None);
            }
            
            protected async Task AssignRole(RoleId roleId, UserId? toUserId, RealmId? inRealmId = null)
            {
                var command = new AssignRoleCommand
                {
                    RoleId = roleId,
                    ToUserId = toUserId,
                    InRealmId = inRealmId
                };
            
                await AssignRoleAction.ExecuteAsync(command, ActionId, CancellationToken.None);
            }
            
            protected async Task Authenticate(Email email, string password)
            {
                var command = new AuthenticateCommand
                {
                    Email = email,
                    Password = password
                };
            
                var accessToken = await AuthenticateAction.ExecuteAsync(command, ActionId, CancellationToken.None);

                Credentials.JwtToken = JwtToken.Read(accessToken.ToString());
            }
            
            protected async Task AuthenticateRootUser()
            {
                var command = new AuthenticateCommand
                {
                    Email = CustomSettings.RootUser.Email,
                    Password = CustomSettings.RootUser.Password
                };
            
                var accessToken = await AuthenticateAction.ExecuteAsync(command, ActionId, CancellationToken.None);

                Credentials.JwtToken = JwtToken.Read(accessToken.ToString());
            }
            
            protected async Task AuthenticateUser(string password = "test-password")
            {
                var command = new AuthenticateCommand
                {
                    Email = User.Email,
                    Password = password
                };
            
                var accessToken = await AuthenticateAction.ExecuteAsync(command, ActionId, CancellationToken.None);

                Credentials.JwtToken = JwtToken.Read(accessToken.ToString());
            }

            protected async Task CreateAccount(string email = "test.testsson@poweriam.com", string password = "test-password")
            {
                var command = new CreateAccountCommand
                {
                    FirstName = "Test",
                    LastName = "Testsson",
                    Email = Email.Create(email),
                    Password = password,
                    RepeatPassword = password
                };
            
                var user = await CreateAccountAction.ExecuteAsync(command, ActionId, CancellationToken.None);

                Users.Add(user);
            }
            
            protected async Task CreateDomain(RealmId inRealmId, string name = "Test Domain", string description = "Test description")
            {
                var command = new CreateDomainCommand
                {
                    Name = name,
                    Description = description,
                    InRealmId = inRealmId
                };
            
                var domain = await CreateDomainAction.ExecuteAsync(command, ActionId, CancellationToken.None);

                Domains.Add(domain);
            }
            
            protected async Task CreatePermission(string name = "Test Permission", RealmId? inRealmId = null, DomainId? inDomainId = null)
            {
                var command = new CreatePermissionCommand
                {
                    Name = name,
                    Description = "Test Permission",
                    ExternalId = "some-external-id",
                    InRealmId = inRealmId,
                    InDomainId = inDomainId
                };
            
                var permission = await CreatePermissionAction.ExecuteAsync(command, ActionId, CancellationToken.None);

                Permissions.Add(permission);
            }
            
            protected async Task CreateRealm(string name = "Test Realm")
            {
                var command = new CreateRealmCommand
                {
                    Name = name,
                    Description = "Test Realm",
                    ExternalId = "some-external-id"
                };
            
                var realm = await CreateRealmAction.ExecuteAsync(command, ActionId, CancellationToken.None);

                Realms.Add(realm);
            }
            
            protected async Task CreateRole(string name = "Test Permission", RealmId? inRealmId = null, string? inExternalRealmId = null)
            {
                var command = new CreateRoleCommand
                {
                    Name = name,
                    Description = "Test Role",
                    InRealmId = inRealmId,
                    InExternalRealmId = inExternalRealmId
                };
            
                var role = await CreateRoleAction.ExecuteAsync(command, ActionId, CancellationToken.None);

                Roles.Add(role);
            }
            
            protected async Task<IEnumerable<Assignment>> GetRoleAssignments(UserId toUserId, RealmId? inRealmId = null)
            {
                var command = new GetRoleAssignmentsCommand
                {
                    ToUserId = toUserId,
                    InRealmId = inRealmId
                };
            
                var assignments = await GetRoleAssignmentsAction.ExecuteAsync(command, ActionId, CancellationToken.None);

                return assignments;
            }
            
            // Data

            protected async Task Refresh(User user)
            {
                var users = new List<User>();
                foreach (var u in Users)
                    if (u.UserId == user.UserId)
                        users.Add(await UserRepository.GetAsync(u.UserId, ActionId, CancellationToken.None));
                    else
                        users.Add(u);
                Users = users;
            }
        }
    }


###############
Troubleshooting
###############

If you suspect something in the nuget isn't working as expected, it will be helpful to increase the logging level of the
framework to the ``DEBUG`` level in the ``env file`` like this::

    CFG_LOGGING_LEVEL=Debug

This should provide useful information about what's going on inside the OpenDDD.NET core.
