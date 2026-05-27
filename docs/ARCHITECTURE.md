# Architectural Specifications

This document outlines the architectural boundaries, dependency flows, SOLID principles implementation, and database mappings of the HoraMarcada backend.

---

## 1. Clean Architecture Boundaries

The backend is built following Clean Architecture principles, separating core business rules from infrastructure details and delivery frameworks.

```
       ┌──────────────────────────────────────────────┐
       │                  HoraMarcada.Api                  │
       └──────────────────────┬───────────────────────┘
                              │ Reference
       ┌──────────────────────▼───────────────────────┐
       │             HoraMarcada.Infrastructure            │
       └──────────────────────┬───────────────────────┘
                              │ Reference
       ┌──────────────────────▼───────────────────────┐
       │              HoraMarcada.Application              │
       └──────────────────────┬───────────────────────┘
                              │ Reference
       ┌──────────────────────▼───────────────────────┐
       │                HoraMarcada.Domain                 │
       └──────────────────────────────────────────────┘
```

### Layer Details & Directory Structure

#### 1. HoraMarcada.Domain
- **Role**: Core business domain model. Holds enterprise invariants, entity definitions, value objects, and domain exceptions.
- **Dependencies**: Zero runtime dependencies. No references to database libraries (EF Core), HTTP layers, or external frameworks.
- **Key Path**: [src/HoraMarcada.Domain](../src/HoraMarcada.Domain)

#### 2. HoraMarcada.Application
- **Role**: Use-case orchestration. Translates external requests into domain workflows. Declares repository interfaces (ports) and defines input/output DTO structures.
- **Dependencies**: Depends ONLY on `HoraMarcada.Domain`.
- **Key Path**: [src/HoraMarcada.Application](../src/HoraMarcada.Application)

#### 3. HoraMarcada.Infrastructure
- **Role**: Gateway to external dependencies (database persistence, security providers, messaging systems). Implements repository ports and configures ORM mappings.
- **Dependencies**: Depends on `HoraMarcada.Application` and `HoraMarcada.Domain`.
- **Key Path**: [src/HoraMarcada.Infrastructure](../src/HoraMarcada.Infrastructure)

#### 4. HoraMarcada.Api
- **Role**: Presentation and composition root. Exposes HTTP endpoints, configures the ASP.NET routing pipeline, parses and validates incoming payloads, maps exceptions to HTTP responses, and handles JWT authorization checks.
- **Dependencies**: Depends on both `HoraMarcada.Infrastructure` and `HoraMarcada.Application`.
- **Key Path**: [src/HoraMarcada.Api](../src/HoraMarcada.Api)

---

## 2. SOLID Principles Implementation

The codebase enforces SOLID principles to remain flexible, testable, and simple.

### Single Responsibility Principle (SRP)
- **Use Cases**: Each use case is encapsulated in a single class with a single public method (e.g., `CreateStoreUseCase.ExecuteAsync`), representing exactly one execution flow.
- **Controllers**: Handlers are organized into a "Controller-Per-Action" pattern. Every HTTP route gets its own dedicated folder containing a singular controller class and its validator class (e.g., `CreateStoreController` and `CreateStoreRequestValidator`).

### Open-Closed Principle (OCP)
- Core logic is written so that behavior can be extended without modifying the existing execution engine. For example, in the availability logic, slots are calculated based on polymorphic collections like lists of professional schedules, and scheduling calculations do not require changing the entity schemas.

### Liskov Substitution Principle (LSP)
- **Repository Abstractions**: Concrete adapters like `EfStoreRepository` can be substituted transparently with a `Mock<IStoreRepository>` inside unit tests. Both satisfy all conditions required by the consumer use cases without changing behavior or causing exceptions.

### Interface Segregation Principle (ISP)
- Repository interfaces are highly cohesive and narrow. Rather than exposing a monolithic `IUnitOfWork` or `IDatabaseContext` to the application logic, each domain aggregate has its own interface:
  - `IStoreRepository`
  - `IProfessionalRepository`
  - `IServiceRepository`
  - `IAppointmentRepository`

### Dependency Inversion Principle (DIP)
- High-level orchestrators (`UseCases`) depend directly on interface abstractions (`IRepository`), not on concrete SQL implementations (`EfRepository`).
- Dependency injection composition is split into extension methods:
  - `AddApplication()` in `HoraMarcada.Application`
  - `AddInfrastructure()` in `HoraMarcada.Infrastructure`

---

## 3. Database Mapping & ORM Configurations

Entity Framework Core maps the Domain Entities to the database without exposing the persistence model to the Domain layer.

### Collection Encapsulation
To protect the integrity of lists inside aggregates, entities store relationships in private backing fields and expose them via read-only public properties.
Example from `Store`:
```csharp
private readonly List<Professional> _professionals = [];
public IReadOnlyCollection<Professional> Professionals => _professionals.AsReadOnly();
```

In the configuration (`StoreConfiguration`):
```csharp
var professionalsNavigation = builder.Metadata.FindNavigation(nameof(Store.Professionals))!;
professionalsNavigation.SetField("_professionals");
professionalsNavigation.SetPropertyAccessMode(PropertyAccessMode.Field);
```

### Value Object Converters
Value objects like `Slug`, `PhoneNumber`, and `Money` are stored as primitive types in the PostgreSQL database. EF Core uses custom conversions to reconstruct the value objects:
```csharp
builder.Property(s => s.Slug)
    .HasColumnName("slug")
    .HasConversion(ValueObjectConverters.SlugConverter)
    .IsRequired();
```

These converters are defined centrally in:
[ValueObjectConverters.cs](../src/HoraMarcada.Infrastructure/Persistence/Converters/ValueObjectConverters.cs)

---

## 4. Key External Integrations

- **Supabase Auth**: Incoming requests supply a JWT in the `Authorization` header. The middleware validates this token using the JWKS endpoint provided by Supabase:
  `https://{project}.supabase.co/auth/v1/user`
- **Supabase Row Level Security (RLS)**: Secures direct database access. Store owners can only query stores where `user_id = auth.uid()`. Check [supabase-rls.sql](../scripts/supabase-rls.sql) for execution details.
- **Asynchronous Notification Pipeline**: Emails and notifications are dispatched asynchronously via the `INotificationService` to keep API response times minimal. We use the **Decorator Pattern** (`QueueNotificationServiceDecorator`) to intercept calls to `INotificationService` from the Use Cases and immediately place a job onto an in-memory queue (`ChannelNotificationQueue` using `System.Threading.Channels`). A background worker (`NotificationQueueProcessor` running as a `BackgroundService`) dequeues these jobs, resolves the concrete `ResendEmailNotificationService` in a scoped container, and dispatches the e-mail with a linear backoff retry policy (maximum of 3 attempts) in case of transient API errors.
- **Hangfire**: Triggers automatic WhatsApp reminders and summary dispatches.
- **WhatsApp Gateway**: Communicates using Z-API / Evolution API (MVP) or Twilio (production) integration.

