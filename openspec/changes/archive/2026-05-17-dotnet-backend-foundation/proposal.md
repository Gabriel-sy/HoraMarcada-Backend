## Why

The appointment scheduling product defined in the PRD requires an ASP.NET Core 8 backend, but the repository has no .NET solution yet. Without a layered foundation and domain model aligned to the PRD data model, every subsequent feature (CRUD, availability, WhatsApp, Hangfire) would be built on ad-hoc structure and violate dependency inversion. This change establishes the minimum correct architecture so feature work can proceed incrementally with clear boundaries between business rules and infrastructure.

## What Changes

- Create a .NET solution with Clean Architecture layers: Domain, Application, Infrastructure, API
- Scaffold ASP.NET Core 8 Web API host project with baseline configuration (no feature endpoints yet)
- Add core NuGet packages for PostgreSQL (EF Core + Npgsql), JWT auth (Supabase), and solution-wide analyzers
- Implement Domain layer: entities, value objects, enums, and repository interfaces matching PRD entities (Store, Professional, WeeklyHours, Service, Appointment)
- Enforce Dependency Inversion: Domain has zero external dependencies; Application defines use-case ports; Infrastructure and API depend inward
- Separate **policies** (domain rules, invariants, authorization ownership) from **details** (EF mappings, Supabase JWT, HTTP, WhatsApp, Hangfire) — details live only in outer layers
- Add solution structure, `.editorconfig`, and README with build/run instructions

## Capabilities

### New Capabilities

- `solution-structure`: .NET solution layout, projects, references, and DIP dependency rules
- `domain-model`: Core entities, value objects, enums, and repository interfaces derived from the PRD data model

### Modified Capabilities

- _(none — greenfield project)_

## Impact

- **New code**: Entire backend solution under `src/` (or root-level projects per design)
- **Dependencies**: .NET 8 SDK, EF Core, Npgsql, JWT Bearer packages (versions pinned in design)
- **Systems**: Prepares for PostgreSQL via Supabase and Supabase Auth JWT validation in later changes; not wired in this change beyond package references and project placeholders
- **Out of scope for this change**: API endpoints, EF DbContext/migrations, WhatsApp, Hangfire jobs, availability algorithm, integration tests against Supabase
