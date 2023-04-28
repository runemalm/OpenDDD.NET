OpenDDD.NET
===========

Welcome to the OpenDDD.NET framework documentation!

This framework is used to do DDD with .NET. Below is an example of a bounded context implemented with OpenDDD.NET.

Check out the :doc:`user guide<gettingstarted>` to quickly get started building your own context.


Examples
========

These are examples of how your code will look like::

   /* Program.cs */

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
   
   namespace Main
   {
       public class Startup
       {
            /* ... */

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

           /* ... */
       }
   }

::

   /* CreateAccountAction.cs */
   
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

::

   /* User.cs */
   
   namespace Domain.Model.User
   {
       public class User : Aggregate, IAggregate, IEquatable<User>
       {
           public UserId UserId { get; set; }
           public string FirstName { get; set; }
           public string LastName { get; set; }
           public Email Email { get; set; }
           public DateTime? EmailVerifiedAt { get; set; }

           /* ... */
           
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

               /* ... */
           }
           
           /* ... */
       }
   }

::

   # Configuration (env file):
   
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


.. gettingstarted-docs:
.. toctree::
  :maxdepth: 1
  :caption: User guide

  gettingstarted

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
