## 1. Project Overview

**HoraMarcada** is an online scheduling SaaS system aimed at small service businesses (e.g., barbershops, salons, manicurists). Its focus is on extreme simplicity for customers and automation via WhatsApp integrations for confirmations, reminders, and cancellations.

### User Roles
- **Entrepreneur (Admin)**: Registers the store, manages services, professionals, weekly hours, and schedules via the web panel. Authenticated via Supabase.
- **Professional**: Hairdressers, barbers, etc. In the MVP, they don't have separate panels but receive schedule summaries and cancellation notices via WhatsApp.
- **Customer**: Schedules appointments via a public page (`domain.com/[slug]`) in a 4-step wizard without registering or logging in.

---

## 2. Technical Stack

- **Framework**: ASP.NET Core 8 Web API
- **ORM**: Entity Framework Core 8
- **Database**: PostgreSQL (hosted on Supabase)
- **Background Jobs**: Hangfire (reminders and daily summaries)
- **Authentication**: JWT Bearer Validation (Supabase Auth)
- **Validation**: FluentValidation
- **Testing**: xUnit & Moq

---

## 3. Agent Task Routing Matrix

Use this table to find where you need to go in the repository based on the task assigned to you:

| Task Type | Target Directory / File | Description |
| :--- | :--- | :--- |
| **Understand Business Rules** | [BUSINESS_LOGIC.md](BUSINESS_LOGIC.md) | High-level requirements and domain model details. |
| **Modify Business Invariants** | [HoraMarcada.Domain](../src/HoraMarcada.Domain) | Entities, Value Objects, and domain exceptions. |
| **Add New Features / Core Logic** | [HoraMarcada.Application/UseCases](../src/HoraMarcada.Application/UseCases) | Core application use cases and business orchestration. |
| **Define Persistence Contracts** | [HoraMarcada.Application/Repositories](../src/HoraMarcada.Application/Repositories) | Repository interface declarations (ports). |
| **Implement Database Access** | [HoraMarcada.Infrastructure/Persistence/Repositories](../src/HoraMarcada.Infrastructure/Persistence/Repositories) | EF Core implementations of repository interfaces (adapters). |
| **Configure DB Schemas / Mappings** | [HoraMarcada.Infrastructure/Persistence/Configurations](../src/HoraMarcada.Infrastructure/Persistence/Configurations) | Fluent API entity mapping configurations. |
| **Add Database Migrations** | [HoraMarcada.Infrastructure/Migrations](../src/HoraMarcada.Infrastructure/Migrations) | EF Core schema migrations. |
| **Add API Endpoints** | [HoraMarcada.Api/Controllers](../src/HoraMarcada.Api/Controllers) | Controller classes grouped by resource. |
| **Define API Request Validation** | [HoraMarcada.Api/Controllers](../src/HoraMarcada.Api/Controllers) | FluentValidation request validators. |
| **Write/Run Unit Tests** | [HoraMarcada.UnitTests](../tests/HoraMarcada.UnitTests) | Test suites mirroring use cases. |
| **Manage Background Jobs** | [HoraMarcada.Infrastructure/DependencyInjection](../src/HoraMarcada.Infrastructure/DependencyInjection) | Hangfire initialization and registration. |

---

## 4. Documentation Index

The following specific documents provide details for each area of the application:

1. **[ARCHITECTURE.md](ARCHITECTURE.md)**: Details Clean Architecture project layouts, dependencies, SOLID principles implementation, and the physical architecture.
2. **[BUSINESS_LOGIC.md](BUSINESS_LOGIC.md)**: Details the domain model (Entities and Value Objects), key scheduling slot availability algorithms, and cancellation token logic.
3. **[DEVELOPMENT_GUIDELINES.md](DEVELOPMENT_GUIDELINES.md)**: Coding standards, domain instantiation patterns, Controller-Per-Action design, validation, and error handling.
4. **[OPENSPEC_GUIDE.md](OPENSPEC_GUIDE.md)**: Introduces the OpenSpec specification changes workflow and lists key commands.
5. **[TESTING.md](TESTING.md)**: Guidance on writing unit tests, structuring AAA patterns, using mocks, and CLI commands.

---

## 5. Core Rules for AI Agents

When working on this repository, you must adhere strictly to these global constraints:

- **Update Docs**: After changing code, check if docs need updating, if so, update them accordingly.
- **SOLID Principles**: Respect single responsibility, ensure interfaces are segregation-friendly, and leverage dependency injection.
- **Keep it Simple**: Avoid unnecessary complexity. If a simple solution fulfills the requirement, prioritize it over abstract patterns.
