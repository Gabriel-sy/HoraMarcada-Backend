## 1. Packages and project setup

- [x] 1.1 Add `Microsoft.EntityFrameworkCore.Design`, `Microsoft.Extensions.Configuration.Json`, and `Microsoft.Extensions.Configuration.EnvironmentVariables` to `Directory.Packages.props`
- [x] 1.2 Reference Design (PrivateAssets) and configuration packages in `HoraMarcada.Infrastructure.csproj`
- [x] 1.3 Add `Microsoft.EntityFrameworkCore.Design` reference to `HoraMarcada.Api` for EF CLI startup project support

## 2. DbContext and persistence structure

- [x] 2.1 Create `Persistence/HoraMarcadaDbContext.cs` with DbSets for Store, Professional, WeeklyHours, Service, ServiceProfessional (join), Appointment
- [x] 2.2 Apply all `IEntityTypeConfiguration<>` classes from assembly in `OnModelCreating`
- [x] 2.3 Add `HoraMarcadaDesignTimeDbContextFactory` loading config from `HoraMarcada.Api` appsettings

## 3. Entity configurations

- [x] 3.1 Add `StoreConfiguration` (table `stores`, value converters for Slug and PhoneNumber, column names per PRD)
- [x] 3.2 Add `ProfessionalConfiguration` (table `professionals`, optional PhoneNumber converter)
- [x] 3.3 Add `WeeklyHoursConfiguration` (table `weekly_hours`, unique index on professional_id + day_of_week, TimeOnly columns)
- [x] 3.4 Add `ServiceConfiguration` (table `services`, Money converter for price)
- [x] 3.5 Add `ServiceProfessionalConfiguration` (table `service_professional`, composite key)
- [x] 3.6 Add `AppointmentConfiguration` (table `appointments`, status as text/check, indexes, cancel_token unique)

## 4. Supabase connection and DI

- [x] 4.1 Update `InfrastructureServiceCollectionExtensions.AddInfrastructure(IServiceCollection, IConfiguration)` to register `DbContext` with `UseNpgsql(ConnectionStrings:Supabase)`
- [x] 4.2 Validate connection string is present; throw `InvalidOperationException` with clear message if missing
- [x] 4.3 Update `HoraMarcada.Api` `Program.cs` to pass `builder.Configuration` into `AddInfrastructure`

## 5. Migrations

- [x] 5.1 Run `dotnet ef migrations add InitialCreate` (Infrastructure + Api startup)
- [x] 5.2 Review generated migration against PRD section 5.2 (tables, FKs, indexes)
- [x] 5.3 Add `scripts/supabase-rls.sql` with PRD section 5.3 policies for manual Supabase execution

## 6. Repository implementations

- [x] 6.1 Implement `EfStoreRepository` (GetById, GetBySlug with includes as needed, Add + SaveChanges)
- [x] 6.2 Implement `EfProfessionalRepository` (GetById with WeeklySchedules, ListByStoreId, Add)
- [x] 6.3 Implement `EfServiceRepository` (GetById with join for ProfessionalIds, ListByStoreId, Add)
- [x] 6.4 Implement `EfAppointmentRepository` (GetById, Add)
- [x] 6.5 Register all repository implementations as scoped in `AddInfrastructure`

## 7. Documentation and verification

- [x] 7.1 Document Supabase connection string setup, user secrets, and `dotnet ef` commands in README
- [x] 7.2 Run `dotnet build` and confirm solution compiles
- [x] 7.3 Apply migration to local or Supabase Postgres and verify tables exist (manual or documented step)
