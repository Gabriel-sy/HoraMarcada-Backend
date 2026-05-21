## ADDED Requirements

### Requirement: Application service interfaces defined

The Application layer SHALL define interfaces for store, professional, service catalog, availability, and appointment workflows. Each interface SHALL expose async methods aligned to PRD use cases and SHALL NOT reference Entity Framework, ASP.NET Core, HTTP, Supabase, WhatsApp, or Hangfire types.

#### Scenario: Interfaces live in Application project

- **WHEN** inspecting `Marque.Application` service interfaces
- **THEN** they are declared under `Services/` and reference only domain types and application models

### Requirement: Use cases registered in AddApplication

`AddApplication()` SHALL register each application service interface with its concrete implementation using scoped lifetime, along with `IStoreOwnershipPolicy` and `ITimeProvider`.

#### Scenario: DI resolves store service

- **WHEN** the host calls `services.AddApplication()` and resolves `IStoreService` from a built service provider
- **THEN** a non-null implementation is returned without referencing Infrastructure types in Application

### Requirement: API depends on abstractions only

Outer layers (API) SHALL depend on application service interfaces, not concrete use-case classes. Concrete classes SHALL be internal or public implementations registered only via DI extension.

#### Scenario: Application has no Infrastructure reference

- **WHEN** inspecting `Marque.Application.csproj` project references
- **THEN** it does not reference `Marque.Infrastructure`
