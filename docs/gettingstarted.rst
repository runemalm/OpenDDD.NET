############
Installation
############

Install the framework using the package manager or the `dotnet CLI <https://learn.microsoft.com/en-us/nuget/consume-packages/install-use-packages-dotnet-cli>`_::

    $ dotnet add package OpenDDD.NET


###################
Example Application
###################

The quickest way to get a taste of this framework is to check out the code snippets on the :doc:`start page<index>`.

If you want to see the full source code of a project based on this framework you can check out the `poweriam github repository <https://...>`_.

Another easy way to get started quickly is to use the WeatherForecast `project templates <https://todo>`_.

Next, we'll guide you through the building blocks to get you started.


###############
Building Blocks
###############

We will now cover all the ``building blocks`` of the framework:

* :ref:`Env files`
* :ref:`DomainModelVersion`
* :ref:`Actions`
* :ref:`Entities`
* :ref:`Events`
* :ref:`Listeners`
* :ref:`Domain Services`
* :ref:`Errors`
* :ref:`Serializers`
* :ref:`Migrators`
* :ref:`Unit Tests`
* :ref:`The Action base class`
* :ref:`The ActionDependencies class`


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

    # Service Bus
    CFG_SERVICEBUS_CONN_STR=
    CFG_SERVICEBUS_SUB_NAME=

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


DomainModelVersion
------------------

Since this framework is all about focusing on an evolving and up-to-date domain model, we need to have a representation of a domain model ``version``.

Create this class by subclassing the ``DomainModelVersion`` base class.

As your model evolves, you will increment the ``LatestString`` and add appropriate migration methods to the entity migrators. More on :ref:`migrators in a later section <Migrators>`.

Example DomainModelVersion::

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


Errors
---------------

Describe this...


Serializers
-----------

Describe this..


Migrators
---------

Describe this..


Unit Tests
----------

Describe this..


The Action base class
---------------------

Describe this..


The ActionDependencies class
----------------------------

Describe this..


###############
Troubleshooting
###############

If you suspect something in the ddd package isn't as expected, it will be helpful to increase the logging level of the
framework to the ``DEBUG`` level in the ``env file`` like this::

    CFG_LOGGING_LEVEL=Debug

This should provide lots of useful information about what's going on inside the openddd.net core.
