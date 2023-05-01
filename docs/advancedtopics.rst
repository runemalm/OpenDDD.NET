###################
Semantic Versioning
###################

The SemVer2.0 policy is used for versioning of the domain model, as well as the HTTP API of the primary http adapter, and this nuget.

In SemVer2.0, *backwards compatible* changes increments the patch- and minor versions, whereas *backwards incompatible* changes increments the major version.

See the table for examples how this works.

.. list-table:: Examples: When to increment which version.
   :widths: 25 25 50 50
   :header-rows: 1

   * - Code Status
     - Stage
     - Rule
     - Example version
   * - First release
     - New product
     - Start with 1.0.0
     - 1.0.0
   * - Backward-compatible bug fixes
     - Patch release
     - Increment the third digit
     - 1.0.1
   * - Backward-compatible new features
     - Minor release
     - Increment the middle digit and reset last digit to zero
     - 1.1.0
   * - Changes that break backward compatibility
     - Major release
     - Increment the first digit and reset middle and last digits to zero
     - 2.0.0
