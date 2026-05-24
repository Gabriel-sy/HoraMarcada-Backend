## Why

Currently, the application layer use cases lack automated test coverage, making it difficult to verify their correctness and prevent regressions as the system evolves. Adding a robust unit testing suite ensures high code quality, documents business rule behavior, and enables safe refactoring.

## What Changes

- Create a new unit testing project `HoraMarcada.UnitTests` under the `tests` directory.
- Configure `Directory.Packages.props` and the test project to support `xUnit` and `Moq` using Central Package Management (CPM).
- Structure the test project to mirror `HoraMarcada.Application/UseCases` by organizing tests into folders matching the use case namespaces: `Appointments`, `Availability`, `Professionals`, `Services`, and `Stores`.
- Write comprehensive unit tests for all application use cases using the Arrange-Act-Assert (AAA) pattern.
- Establish clean test naming conventions (e.g., `ExecuteAsync_WhenCondition_ShouldExpectedResult`).

## Capabilities

### New Capabilities
- `application-unit-tests`: A comprehensive suite of unit tests for application layer use cases ensuring all paths (success, validation, not found, exceptions) are covered.

### Modified Capabilities

## Impact

- Solution structure: Added a new `tests` folder and a `HoraMarcada.UnitTests.csproj` project reference in `HoraMarcada.sln`.
- Dependency management: Added test dependencies (`xunit`, `moq`, `xunit.runner.visualstudio`, `Microsoft.NET.Test.Sdk`, `coverlet.collector`) to `Directory.Packages.props`.
- Development workflow: Added automated test execution via `dotnet test`.
