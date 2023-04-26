#######
Testing
#######

The tests are based on the ``xUnit`` design pattern.

Test methods must be written for ``each action`` of the ``domain model``. Each test method implements one or more paths of the action. Since the business actions of the domain are defined fully by these actions, all these test methods together provides a full test coverage of the domain.

More specifically, you will create one class (test suite) for each of your actions. This class will subclass the ``ActionUnitTests`` class and implement test methods for all the paths of the action as described above.

The framework provides ``convenience methods`` for ``asserting`` events are published, emails are sent, aggregate state is changed, etc.

Below is an ``example`` of what an action test will look like::

    using Xunit;
    using Application.Actions.Commands;
    using OpenDDD.Domain.Model.Error;
    using Domain.Model.User;

    namespace Tests.Actions;

    public class CreateAccountTests : ActionUnitTests
    {
        public CreateAccountTests()
        {
            Configure();
            EmptyDb();
        }

        [Fact]
        public async Task TestSuccess_EventPublished()
        {
            // Arrange
            await EnsureRootUserAsync();
            await EnsureIamDomainAsync();
            await EnsureIamPermissionsAsync();
            
            var command = new CreateAccountCommand
            {
                FirstName = "Test",
                LastName = "Testsson",
                Email = Email.Create("test.testsson@poweriam.com"),
                Password = "TestPassword",
                RepeatPassword = "TestPassword"
            };
            
            // Act
            var user = await CreateAccountAction.ExecuteAsync(command, ActionId, CancellationToken.None);
            
            // Assert
            AssertDomainEventPublished(new AccountCreated(user, ActionId));
        }

        /* etc... */
    }


##########
Migrations
##########

Describe this...


##############
Access Control
##############

Describe this...


##########
Automation
##########

Describe this...

Jobs
----

Describe this...


Tasks
-----

Describe this...


Makefile
--------

Describe this...


####################
External Integration
####################

Describe this...


HTTP API
--------

Describe this...


Integration Events
------------------

Describe this...


############################
Command Line Interface (CLI)
############################

Describe this...


#######
Hosting
#######

Describe this...


##############
.NET Framework
##############

Describe how:

- The .NET framework has evolved.
- How it's separated into "host" and "application".
- How it's booted.
- How this relates to OpenDDD startup sequence.


###################
Interchange Context
###################

Describe this...
