## Context

Currently, the `Marque` solution contains four projects: `Marque.Domain`, `Marque.Application`, `Marque.Infrastructure`, and `Marque.Api`. There is no project for automated testing. This proposal designs the setup of the testing infrastructure and implements unit tests for all application layer use cases in `Marque.Application/UseCases`.

## Goals / Non-Goals

**Goals:**
- Set up a new unit test project `Marque.UnitTests` in the `tests/` directory and add it to `Marque.sln`.
- Enable Central Package Management (CPM) for all test-related package dependencies.
- Implement comprehensive unit tests for all use cases in the application layer using `xUnit` and `Moq`.
- Follow the Arrange-Act-Assert (AAA) pattern for readability and maintainability.
- Use a clear naming convention: `ExecuteAsync_When[Scenario]_Should[Expectation]`.
- Organize tests in a directory structure that mirrors `Marque.Application/UseCases`.

**Non-Goals:**
- Setting up integration tests or API/E2E test suites (e.g., using WebApplicationFactory or real databases).
- Implementing tests for Domain entities or Infrastructure layers (unless trivially required; focus is on the Application use cases).

## Decisions

### 1. Test Project Placement and Naming
- **Decision:** Place the project at `tests/Marque.UnitTests/Marque.UnitTests.csproj`.
- **Rationale:** Separating production code (`src/`) from test code (`tests/`) is a standard .NET project structure. Using a single test project for the entire solution avoids project bloat while keeping namespaces separated.

### 2. Central Package Management (CPM) Configuration
- **Decision:** Add the versions of test dependencies to `/Directory.Packages.props` at the solution root.
- **Rationale:** Consistent with how the existing production projects manage their packages.
- **Package Versions to Add:**
  - `Microsoft.NET.Test.Sdk` version `17.11.1`
  - `xunit` version `2.9.2`
  - `xunit.runner.visualstudio` version `2.8.2`
  - `moq` version `4.20.72`
  - `coverlet.collector` version `6.0.2`

### 3. Test Organization
- **Decision:** Folder names in `Marque.UnitTests` will exactly mirror the namespace of the use case class they test.
- **Example:**
  - Class: `Marque.Application.UseCases.Availability.GetAvailableSlotsUseCase`
  - Test File: `tests/Marque.UnitTests/UseCases/Availability/GetAvailableSlotsUseCaseTests.cs`

### 4. Mocking Strategy
- **Decision:** Use Moq (`Mock<T>`) to mock repository interfaces (`IStoreRepository`, `IServiceRepository`, `IProfessionalRepository`, `IAppointmentRepository`) and other external services.
- **Rationale:** Ensures we are testing only the application use case's orchestrating logic and business rules without hitting databases or third-party APIs.

## Risks / Trade-offs

- **[Risk] package version mismatch or duplicate package reference errors** → **Mitigation:** Rely strictly on `Directory.Packages.props` for versioning. Do not specify `Version` in the `.csproj` file of the test project.
- **[Risk] Test maintenance overhead** → **Mitigation:** Strictly adhere to the AAA pattern and use cases' public boundaries to avoid coupling test code to inner implementation details.
