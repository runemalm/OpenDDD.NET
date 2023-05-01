OpenDDD.NET
===========

Welcome to the OpenDDD.NET framework documentation.

This framework is used to implement bounded contexts in software teams with a domain-driven design approach to development.

It's heavily based on hexagonal architecture, so if you're familiar with that pattern you should get started very quickly.

Check out the :doc:`user guide<gettingstarted>` to quickly get started building your own context.


Examples
========

Here are some examples how your code will look like::

    /* Program.cs */

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

::

    /* Startup.cs */
    
    using System.Reflection;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using OpenDDD.Application.Settings;
    using OpenDDD.Application.Settings.Persistence;
    using OpenDDD.NET.Extensions;
    using OpenDDD.NET.Hooks;
    using Main.Extensions;
    using Main.NET.Hooks;
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
                AddConversion(services);
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
                services.AddDomainService<IForecastDomainService, ForecastDomainService>();
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

            private void AddConversion(IServiceCollection services)
            {
                services.AddConversion(_settings);
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

::

    /* CreateAccountAction.cs */
    
    using System.Threading;
    using System.Threading.Tasks;
    using OpenDDD.Application;
    using OpenDDD.Domain.Model.Error;
    using OpenDDD.Infrastructure.Ports.PubSub;
    using Application.Actions.Commands;
    using Domain.Model.User;

    namespace Application.Actions
    {
        public class CreateAccountAction : Action<CreateAccountCommand, User>
        {
            private readonly IDomainPublisher _domainPublisher;
            private readonly IUserRepository _userRepository;
            
            public CreateAccountAction(
                IDomainPublisher domainPublisher,
                IUserRepository userRepository,
                ITransactionalDependencies transactionalDependencies)
                : base(transactionalDependencies)
            {
                _domainPublisher = domainPublisher;
                _userRepository = userRepository;
            }

            public override async Task<User> ExecuteAsync(
                CreateAccountCommand command,
                ActionId actionId,
                CancellationToken ct)
            {
                // Validate
                var existing =
                    await _userRepository.GetWithEmailAsync(
                        command.Email,
                        actionId,
                        ct);

                if (existing != null)
                    throw DomainException.AlreadyExists("user", "email", command.Email);

                // Run
                var user =
                    await User.CreateAccountAsync(
                        userId: UserId.Create(await _userRepository.GetNextIdentityAsync()),
                        firstName: command.FirstName,
                        lastName: command.LastName,
                        email: command.Email,
                        password: command.Password,
                        passwordAgain: command.RepeatPassword,
                        domainPublisher: _domainPublisher,
                        actionId: actionId,
                        ct: ct);

                // Persist
                await _userRepository.SaveAsync(user, actionId, ct);
                
                // Return
                return user;
            }
        }
    }

::

    /* User.cs */
    
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.AspNetCore.WebUtilities;
    using OpenDDD.Application;
    using OpenDDD.Domain.Model.BuildingBlocks.Aggregate;
    using OpenDDD.Domain.Model.BuildingBlocks.Entity;
    using OpenDDD.Domain.Model.Error;
    using OpenDDD.Domain.Model.Validation;
    using OpenDDD.Infrastructure.Ports.Email;
    using OpenDDD.Infrastructure.Ports.PubSub;
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
            
            public static async Task<User> CreateAccountAsync(
                UserId userId,
                string firstName,
                string lastName,
                Email email,
                string password,
                string passwordAgain,
                IDomainPublisher domainPublisher,
                ActionId actionId,
                CancellationToken ct)
            {
                if (password != passwordAgain)
                    throw DomainException.InvariantViolation("The passwords don't match.");
                
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
                        IsSuperUser = false,
                        RealmIds = new List<RealmId>()
                    };
                
                user.SetPassword(password, actionId, ct);
                user.RequestEmailValidation(actionId, ct);

                user.Validate();

                await domainPublisher.PublishAsync(new AccountCreated(user, actionId));

                return user;
            }
            
            public static User CreateDefaultAccountAtIdpLogin(
                UserId userId,
                string firstName,
                string lastName,
                Email email,
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
                        IsSuperUser = false,
                        RealmIds = new List<RealmId>()
                    };
                
                user.SetPassword(Password.Generate(), actionId, ct);

                user.Validate();

                return user;
            }
            
            public static User CreateRootAccountAtBoot(
                UserId userId,
                string firstName,
                string lastName,
                Email email,
                string password,
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
                        IsSuperUser = true,
                        RealmIds = new List<RealmId>()
                    };
                
                user.SetPassword(password, actionId, ct);

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

::

    # Configuration file
    
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
    CFG_HTTP_DOCS_TITLE=Weather API

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
    CFG_EMAIL_PROVIDER=memory
    CFG_EMAIL_SMTP_HOST=
    CFG_EMAIL_SMTP_PORT=
    CFG_EMAIL_SMTP_USERNAME=
    CFG_EMAIL_SMTP_PASSWORD=


.. gettingstarted-docs:
.. toctree::
  :maxdepth: 1
  :caption: User guide

  gettingstarted

.. advancedtopics-docs:
.. toctree::
  :maxdepth: 1
  :caption: Advanced Topics

  advancedtopics

.. philosophy-docs:
.. toctree::
  :maxdepth: 1
  :caption: Philosophy

  philosophy

.. versionhistory-docs:
.. toctree::
  :maxdepth: 1
  :caption: Releases

  versionhistory

.. troubleshooting-docs:
.. toctree::
  :maxdepth: 1
  :caption: Troubleshooting

  troubleshooting

.. community-docs:
.. toctree::
  :maxdepth: 1
  :caption: Community

  community

.. apireference-docs:
.. toctree::
  :maxdepth: 1
  :caption: API Reference

  py-modindex
