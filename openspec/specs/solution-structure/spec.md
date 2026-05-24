## Requirements

### Requirement: Solution contains four layered projects

The repository SHALL contain a .NET 8 solution named `HoraMarcada.sln` with exactly these projects under `src/`: `HoraMarcada.Domain`, `HoraMarcada.Application`, `HoraMarcada.Infrastructure`, and `HoraMarcada.Api`.

#### Scenario: Solution builds successfully

- **WHEN** a developer runs `dotnet build` at the repository root
- **THEN** all four projects compile without errors

### Requirement: Dependency direction enforces DIP

Project references SHALL follow inward-only dependency flow: `HoraMarcada.Api` references `HoraMarcada.Infrastructure` and `HoraMarcada.Application`; `HoraMarcada.Infrastructure` references `HoraMarcada.Application`; `HoraMarcada.Application` references `HoraMarcada.Domain`; `HoraMarcada.Domain` references no other HoraMarcada projects.

#### Scenario: Domain has no infrastructure references

- **WHEN** inspecting `HoraMarcada.Domain.csproj`
- **THEN** it contains no project references to Infrastructure or Api and no packages for EF Core, ASP.NET Core, or Hangfire

### Requirement: Central package management

The solution SHALL use `Directory.Packages.props` at the repository root to centralize NuGet package versions for shared dependencies.

#### Scenario: Package versions are centralized

- **WHEN** a developer opens `Directory.Packages.props`
- **THEN** versions for EF Core, Npgsql, JWT Bearer, and Hangfire packages are defined in one place

### Requirement: API host runs with health endpoint

`HoraMarcada.Api` SHALL be an ASP.NET Core 8 Web API project that starts via `dotnet run` and exposes a health check endpoint returning HTTP 200.

#### Scenario: Health check responds

- **WHEN** the API is running and a client sends `GET /health`
- **THEN** the response status is 200 and the body indicates the service is healthy

### Requirement: Swagger available in development

The API SHALL register OpenAPI/Swagger middleware enabled only in the Development environment.

#### Scenario: Swagger UI in development

- **WHEN** the API runs with `ASPNETCORE_ENVIRONMENT=Development`
- **THEN** Swagger UI is accessible for the API project

### Requirement: Repository documents build steps

The repository SHALL include a README with .NET 8 SDK prerequisite, restore, build, and run instructions for `HoraMarcada.Api`.

#### Scenario: New developer can run locally

- **WHEN** a developer with .NET 8 SDK follows the README
- **THEN** they can restore, build, and run the API successfully
