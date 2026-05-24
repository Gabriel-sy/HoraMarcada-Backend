## Context

Greenfield repository with only `PRD.md`. The product is an appointment scheduling SaaS (HoraMarcada) targeting small service businesses. The PRD specifies ASP.NET Core 8, PostgreSQL via Supabase, Supabase Auth JWT, Hangfire, and WhatsApp integration. This change scaffolds the backend skeleton with Clean Architecture and a domain model aligned to PRD section 5 (data model), without implementing API features yet.

The user requires layered architecture with strict Dependency Inversion Principle (DIP): inner layers define abstractions; outer layers implement them. **Policies** (business rules, invariants, ownership) belong in Domain/Application; **details** (EF Core, HTTP, JWT validation, messaging) belong in Infrastructure/API only.

## Goals / Non-Goals

**Goals:**

- Single .NET solution with four projects and correct reference direction
- Domain project with zero NuGet dependencies beyond optional analyzers
- Entities and value objects matching PRD tables: `Store`, `Professional`, `WeeklyHours`, `Service`, `Appointment`, plus join semantics for service–professional N:N
- Repository interfaces (ports) in Application or Domain per Clean Architecture convention
- API host project that builds and runs (`dotnet run`) with health check only
- Pin package versions in central `Directory.Packages.props`
- README with prerequisites and build commands

**Non-Goals:**

- REST endpoints (public or private)
- EF Core `DbContext`, configurations, migrations
- Supabase JWT middleware wiring
- WhatsApp, Hangfire, availability algorithm
- Unit/integration test projects (follow-up change)
- CI/CD pipeline

## Decisions

### 1. Solution layout and naming

**Decision:** Use solution name `HoraMarcada.sln` with projects under `src/`:

| Project | Responsibility |
|---------|----------------|
| `HoraMarcada.Domain` | Entities, value objects, enums, domain exceptions |
| `HoraMarcada.Application` | Repository interfaces, DTOs for future use cases (minimal now) |
| `HoraMarcada.Infrastructure` | EF Core, external services (empty stub with DI extension) |
| `HoraMarcada.Api` | ASP.NET Core host, `Program.cs`, middleware pipeline |

**Rationale:** Matches PRD stack and common .NET Clean Architecture naming. `HoraMarcada` is the product name from the repo context.

### 2. Where repository interfaces live

**Decision:** Define repository interfaces in `HoraMarcada.Application` (e.g. `IStoreRepository`, `IAppointmentRepository`). Domain stays persistence-ignorant.

**Rationale:** Application layer owns use-case orchestration ports; Domain only knows entities and domain services. Classic Clean Architecture.

**Alternative:** Interfaces in Domain — acceptable but couples domain to persistence vocabulary; Application ports are clearer for this codebase.

### 3. Entity modeling style

**Decision:** Rich domain entities with private setters, factory methods or constructors enforcing invariants, and explicit `AppointmentStatus` enum (`Confirmed`, `Canceled`, `Completed`). Use value objects where PRD implies constraints:

- `Slug` — non-empty, URL-safe pattern
- `PhoneNumber` — WhatsApp fields (basic validation, E.164-friendly)
- `TimeSlot` / `WeeklySchedule` — encapsulate weekly hours rules (start < end, lunch within bounds)
- `Money` — optional for service price

**Rationale:** Keeps policies in Domain; EF mappings in Infrastructure later will not leak validation into controllers.

### 4. Aggregate boundaries

**Decision:**

- `Store` is aggregate root; owns `Professional` and `Service` collections in domain model (navigation properties)
- `Appointment` is its own aggregate root (references `StoreId`, `ProfessionalId`, `ServiceId` by id)
- `WeeklyHours` belongs to `Professional` (child entity)

**Rationale:** Matches PRD relationships and future consistency boundaries for scheduling.

### 5. Packages (pinned via Central Package Management)

**Decision:** Add to solution root `Directory.Packages.props`:

| Package | Project | Purpose |
|---------|---------|---------|
| `Microsoft.AspNetCore.OpenApi` | Api | OpenAPI in dev |
| `Swashbuckle.AspNetCore` | Api | Swagger UI |
| `Microsoft.AspNetCore.Authentication.JwtBearer` | Infrastructure | Future Supabase JWT |
| `Microsoft.EntityFrameworkCore` | Infrastructure | Future persistence |
| `Npgsql.EntityFrameworkCore.PostgreSQL` | Infrastructure | Supabase PostgreSQL |
| `Hangfire.Core`, `Hangfire.AspNetCore`, `Hangfire.PostgreSql` | Infrastructure | Future jobs (reference only, no setup) |

Domain: **no** EF or ASP.NET packages.

**Rationale:** Install dependencies now to avoid version drift; defer configuration to feature changes.

### 6. API bootstrap

**Decision:** Minimal `Program.cs`: controllers registered (empty), Swagger in Development, `MapGet("/health", ...)` returning 200. No auth middleware yet.

**Rationale:** Proves host runs; auth and controllers come in next changes.

### 7. Policies vs details separation

**Decision:**

| Policies (inner) | Details (outer) |
|------------------|-----------------|
| Appointment cannot overlap (domain rule, enforced later in domain service) | SQL queries, indexes |
| Slug uniqueness (domain + eventual DB unique index) | EF configuration |
| Status transitions (`Confirmed` → `Canceled` / `Completed`) | HTTP status codes |
| Store ownership by `UserId` (authorization policy interface) | Supabase JWT parsing, RLS |

**Rationale:** Directly addresses user requirement to separate politics (rules) from details (infrastructure).

### 8. Project references

```
HoraMarcada.Api → HoraMarcada.Infrastructure → HoraMarcada.Application → HoraMarcada.Domain
HoraMarcada.Api → HoraMarcada.Application (for DI registration of handlers later)
```

Infrastructure must not be referenced by Domain or Application.

## Risks / Trade-offs

| Risk | Mitigation |
|------|------------|
| Over-modeling domain before first use case | Keep entities aligned 1:1 with PRD tables; defer domain services until availability/booking changes |
| Hangfire/EF packages added but unused | Document as intentional placeholders in README; no startup registration in this change |
| PRD uses `AgendaApp` naming in examples | Standardize on `HoraMarcada` in code; note in README |
| N:N `service_professional` modeled only as ids on entities initially | Add `Service.AssignProfessional(ProfessionalId)` on aggregate when CRUD change lands |

## Migration Plan

N/A — greenfield. After merge:

1. `dotnet restore`
2. `dotnet build`
3. `dotnet run --project src/HoraMarcada.Api`

No database migration in this change.

## Open Questions

- Confirm product assembly prefix `HoraMarcada` vs `AgendaApp` from PRD section 8 (design assumes `HoraMarcada`)
- Whether `Application` should reference `MediatR` now or in the first feature change (defer to keep foundation minimal)
