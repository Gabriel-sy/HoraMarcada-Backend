## ADDED Requirements

### Requirement: HoraMarcadaDbContext registers all PRD entities

The Infrastructure layer SHALL provide a `HoraMarcadaDbContext` derived from `DbContext` with `DbSet` properties for stores, professionals, weekly hours, services, appointments, and the service–professional join entity.

#### Scenario: DbContext exposes required sets

- **WHEN** inspecting `HoraMarcadaDbContext`
- **THEN** it contains DbSets for all tables defined in PRD section 5.2 except `auth.users`

### Requirement: PostgreSQL provider via Npgsql

The application SHALL configure `HoraMarcadaDbContext` to use PostgreSQL through `UseNpgsql` with a connection string from configuration key `ConnectionStrings:Supabase`.

#### Scenario: Missing connection string fails at startup

- **WHEN** `ConnectionStrings:Supabase` is empty or missing at runtime
- **THEN** service registration or first DbContext resolution fails with a clear configuration error

### Requirement: DbContext registered in dependency injection

`AddInfrastructure()` SHALL register `HoraMarcadaDbContext` with scoped lifetime and bind it to the Supabase connection string from `IConfiguration`.

#### Scenario: DbContext available to repositories

- **WHEN** the API host calls `AddInfrastructure(services, configuration)` and resolves a repository implementation
- **THEN** the repository receives a scoped `HoraMarcadaDbContext` instance
