## OpenDDD.NET

This is a framework for domain-driven design (DDD) using C# and .NET.

Star and/or follow the project to get notifications on new releases.

### Purpose

Domain-driven design is an approach to software development where focus lies on an ever-evolving domain model.

By utilizing the DDD principles and design patterns, hexagonal architecture and other well-known patterns, this framework is especially crafted for domain-driven design and implementing bounded contexts with C# and the .NET framework.

If you're interested in becoming a contributor, please drop us a line or simply create a pull request.

### Key Features

- Domain model versioning.
- Swagger support.
- Code generation support.
- Domain- & Integration events.
- Migration support.
- Based on industry standard design patterns.
- Testing framework.
- Extensible architecture through *ports and adapters*.
- Rider/VS Project templates.

### Design Patterns

The framework is based on the following design patterns:

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

- .NET 5
- .NET Core 3.1

### Documentation

Documentation is available at [readthedocs](https://opendddnet.readthedocs.io/). 

### Installation

Install the nuget in an existing project using the package manager or the dotnet CLI:

    dotnet add package OpenDDD.NET

NOTE: In most cases it's a better idea to use the project templates to get started quickly with some boilerplate code.

### Create a project using project templates

Use the project template to create a project with boiler plate code.

First install the templates package:

    $ dotnet new install OpenDDD.NET-Templates

Then use the project template to setup a new project:

    dotnet new install openddd-net -n MyBoundedContext

Replace *MyBoundedContext* with the name of your bounded context. A folder will be created with the same name and the solution inside will also get this name.

Refer to the [user guide](https://opendddnet.readthedocs.io/en/latest/gettingstarted.html) for more information on where to go from here.

### Examples

Here are some code examples:

#### CreateAccountAction.cs

```c#
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
```

#### User.cs

```c#
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
```

### Contribution
  
If you want to contribute to the code base, create a pull request on the develop branch. Feel very free to reach out to us by email or via social media.

### Roadmap v1.0.0

- [x] GitHub README
- [x] NuGet README
- [x] Quickstart Guide
- [x] Visual Studio Project Templates
- [x] .NET 5 Support
- [x] .NET Core 3.1 Support
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

### Backlog

- [ ] .NET 8 Support
- [ ] .NET 7 Support
- [ ] .NET 6 Support
- [ ] Full Sample Project
- [ ] Full Test Coverage
- [ ] Monitoring support
- [ ] Migration of all aggregates not at latest domain model version
- [ ] Tasks / Jobs support
- [ ] Command Line Interface (CLI)
- [ ] Admin Dashboard

### Release Notes

**1.0.0-alpha.15** - 2023-05-01

- Re-enable previously disabled publisher service.
- Change message bus topic name format for events.

**1.0.0-alpha.14** - 2023-04-30

- Change listeners to wildcard both minor and patch versions.

**1.0.0-alpha.13** - 2023-04-28

- Rename 'Serialization' to 'Conversion'.
- Add 'PositiveIamAdapter' that permits everything.

**1.0.0-alpha.12** - 2023-04-28

- Rename framework to 'OpenDDD.NET'.
- Add project template for .NET Core 3.1.
- Add project template for .NET 5.
- Introduce Transactional and use in Action. (**breaking**)
- Add extension method 'AddDomainService()'.

**1.0.0-alpha.11** - 2023-04-25

- Add support to disable emails in tests.
- Fix code generation templates.
- Replace IApplicationLifetime with IHostApplicationLifetime. (**breaking**)

**1.0.0-alpha.10** - 2023-04-24

- Add more synchronous versions of methods used by tests.
- Break out application error classes.
- Fix minor issue in code generation tool.

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
