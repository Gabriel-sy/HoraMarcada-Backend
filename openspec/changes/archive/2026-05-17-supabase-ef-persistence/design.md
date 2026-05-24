## Context

The `dotnet-backend-foundation` change delivered Clean Architecture projects, domain entities aligned to PRD section 5, and Application repository ports without implementations. EF Core and Npgsql packages are referenced in `HoraMarcada.Infrastructure` but not configured. `appsettings.Development.json` already defines a placeholder `ConnectionStrings:Supabase` value.

Supabase hosts PostgreSQL with optional Row Level Security (PRD section 5.3). The API will connect with a service role or direct connection string for migrations and server-side writes; RLS policies are applied in Supabase SQL, not via EF migrations.

## Goals / Non-Goals

**Goals:**

- `HoraMarcadaDbContext` exposing all PRD tables with PostgreSQL-compatible mappings
- Fluent `IEntityTypeConfiguration<>` classes per aggregate/entity, using snake_case table and column names matching PRD SQL
- Value converters for `Slug`, `PhoneNumber`, `Money`, and `AppointmentStatus`
- Initial EF migration creating schema equivalent to PRD 5.2 (including indexes on appointments)
- `IDesignTimeDbContextFactory` for `dotnet ef` CLI against Supabase/local Postgres
- Register `DbContext` and repository implementations in `AddInfrastructure()`
- EF repository implementations satisfying existing Application interfaces

**Non-Goals:**

- Supabase Auth JWT middleware or `auth.users` mapping
- Applying or generating RLS policies via EF (document manual SQL script reference from PRD)
- API controllers, use cases, or MediatR handlers
- Hangfire storage tables
- Seed data, integration tests, or CI migration automation
- Changing domain entity behavior or Application interface signatures

## Decisions

### 1. Persistence folder layout

**Decision:** Under `src/HoraMarcada.Infrastructure/Persistence/`:

| Subfolder | Contents |
|-----------|----------|
| `HoraMarcadaDbContext.cs` | DbContext + DbSets |
| `Configurations/` | One configuration class per entity |
| `Converters/` | Shared value converters (optional) |
| `Repositories/` | EF implementations of Application ports |
| `Migrations/` | Generated migration artifacts |

**Rationale:** Keeps Infrastructure organized; matches common .NET Clean Architecture conventions.

### 2. Table and column naming

**Decision:** Use EF `ToTable("stores")`, `HasColumnName("user_id")`, etc., to match PRD snake_case PostgreSQL schema exactly.

**Rationale:** Supabase SQL and RLS policies reference these names; avoids mismatch when applying PRD scripts or inspecting DB in Supabase dashboard.

**Alternative:** PascalCase in DB — rejected because PRD and RLS examples use snake_case.

### 3. Service–professional N:N

**Decision:** Map `service_professional` as a join entity `ServiceProfessional` with composite key `(ServiceId, ProfessionalId)`. Configure many-to-many from `Service` using `ProfessionalIds` via skip navigation or explicit join entity loaded with `Include`.

**Rationale:** PRD defines explicit join table; domain stores `HashSet<Guid>` on `Service` — EF maps via join entity and custom repository load logic.

### 4. Weekly hours persistence

**Decision:** `WeeklyHours` is a separate table `weekly_hours` with FK to `professionals`, unique index on `(professional_id, day_of_week)` per PRD.

**Rationale:** Matches PRD; not owned type (has its own UUID primary key).

### 5. Value object mapping

**Decision:**

| Value object | Storage |
|--------------|---------|
| `Slug` | `text`, converter to/from string |
| `PhoneNumber` | `text`, nullable where entity allows |
| `Money` | `numeric(10,2)`, nullable |
| `AppointmentStatus` | `text` with check constraint via migration or configuration |

**Rationale:** Keeps domain types in entities; DB types match PRD.

### 6. Connection configuration

**Decision:** Read `ConnectionStrings:Supabase` from configuration. Support environment variable override `ConnectionStrings__Supabase` (standard .NET config). Register:

```csharp
services.AddDbContext<HoraMarcadaDbContext>(options =>
    options.UseNpgsql(connectionString));
```

**Rationale:** Already present in `appsettings.Development.json`; works with Supabase connection pooler (Session or Transaction mode URL from Supabase dashboard).

**Supabase note:** Use the direct connection string or pooler URL from Project Settings → Database; store in user secrets or `.env` locally, never commit secrets.

### 7. Design-time factory

**Decision:** `HoraMarcadaDesignTimeDbContextFactory` in Infrastructure implements `IDesignTimeDbContextFactory<HoraMarcadaDbContext>`, builds configuration from `../HoraMarcada.Api/appsettings.json` + `appsettings.Development.json` + environment variables.

**Rationale:** `dotnet ef migrations add` runs with startup project `HoraMarcada.Api` and project `HoraMarcada.Infrastructure`.

### 8. Repository implementation scope

**Decision:** Implement all four repositories with minimal methods required by interfaces: get by id, list by store (where applicable), add, and `SaveChangesAsync` via unit-of-work on `DbContext` (repositories call `SaveChangesAsync` on add, or expose explicit save — prefer single `SaveChangesAsync` per repository method that mutates).

**Rationale:** Unblocks first CRUD/feature change without stub repositories.

**Loading strategy:** `GetById` / `GetBySlug` on store include professionals and services when needed for aggregate reads; professional get includes `WeeklySchedules`; service get includes join for `ProfessionalIds`.

### 9. Migrations vs RLS

**Decision:** Initial migration contains tables, FKs, unique constraints, and indexes from PRD 5.2. Ship a `scripts/supabase-rls.sql` (or document in README) with PRD 5.3 RLS policies for manual execution in Supabase SQL editor.

**Rationale:** EF does not manage RLS; Supabase best practice is SQL policies separate from app migrations.

### 10. Packages

**Decision:** Add to `Directory.Packages.props` and Infrastructure:

- `Microsoft.EntityFrameworkCore.Design` (PrivateAssets) on Infrastructure for tooling
- `Microsoft.Extensions.Configuration.Json` and `Microsoft.Extensions.Configuration.EnvironmentVariables` for design-time factory

Api project references Design package only if needed as startup project (typically `--startup-project HoraMarcada.Api`).

## Risks / Trade-offs

| Risk | Mitigation |
|------|------------|
| Domain private setters block EF materialization | Use private parameterless constructors (already present) and `HasField` / backing fields only if needed; prefer `PropertyAccessMode.Field` during configuration if required |
| `ProfessionalIds` not a navigation property | Map join table explicitly; repository hydrates ids after query |
| Supabase pooler breaks migrations | Document using direct connection (port 5432) for `dotnet ef database update` |
| Schema drift from PRD | Review generated migration against PRD 5.2 before apply |
| API uses service role bypassing RLS | Acceptable for MVP backend; document that RLS protects direct Supabase client access |

## Migration Plan

1. Configure Supabase project and copy database connection string to user secrets or environment
2. `dotnet ef migrations add InitialCreate --project src/HoraMarcada.Infrastructure --startup-project src/HoraMarcada.Api`
3. `dotnet ef database update --project src/HoraMarcada.Infrastructure --startup-project src/HoraMarcada.Api`
4. Run `scripts/supabase-rls.sql` in Supabase SQL editor (manual)
5. Verify tables in Supabase Table Editor

Rollback: `dotnet ef database update 0` then remove migration folder if needed.

## Open Questions

- Whether production uses Supabase connection pooler (6543) or direct (5432) for runtime — recommend pooler for API, direct for migrations
- Confirm `auth.users` FK on `stores.user_id` is enforced only at application level for MVP (no EF relationship to auth schema)
