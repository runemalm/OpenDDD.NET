###############
Version history
###############

.. note:: We are still in alpha stage.

.. tip:: Subscribe to the github repository to get notifications when we move into beta and rc phases!

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
