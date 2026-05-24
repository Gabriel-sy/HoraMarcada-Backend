## 1. Solution scaffolding

- [x] 1.1 Create `HoraMarcada.sln` and four projects under `src/` (`HoraMarcada.Domain`, `HoraMarcada.Application`, `HoraMarcada.Infrastructure`, `HoraMarcada.Api`) targeting `net8.0`
- [x] 1.2 Wire project references per DIP: Api → Infrastructure + Application; Infrastructure → Application; Application → Domain
- [x] 1.3 Add root `Directory.Build.props` and `Directory.Packages.props` with Central Package Management enabled
- [x] 1.4 Add `.editorconfig` with C# conventions consistent across projects

## 2. Package installation

- [x] 2.1 Pin versions in `Directory.Packages.props` for EF Core, Npgsql, JwtBearer, Hangfire, Swashbuckle, OpenApi
- [x] 2.2 Reference packages in `HoraMarcada.Infrastructure` (EF Core, Npgsql, JwtBearer, Hangfire packages — no registration yet)
- [x] 2.3 Reference packages in `HoraMarcada.Api` (OpenApi, Swashbuckle)
- [x] 2.4 Verify `HoraMarcada.Domain` and `HoraMarcada.Application` have no infrastructure packages

## 3. Domain layer

- [x] 3.1 Add `AppointmentStatus` enum (`Confirmed`, `Canceled`, `Completed`)
- [x] 3.2 Implement value objects: `Slug`, `PhoneNumber`, `Money` (or equivalent) with validation
- [x] 3.3 Implement `Store` aggregate root with invariants (name, slug, whatsapp, user id)
- [x] 3.4 Implement `Professional` entity with store association
- [x] 3.5 Implement `WeeklyHours` entity with day-of-week and time-range validation (including lunch bounds)
- [x] 3.6 Implement `Service` entity with duration/price rules and professional association support
- [x] 3.7 Implement `Appointment` entity with status, time range, customer fields, cancel token fields
- [x] 3.8 Add domain-specific exception type(s) for validation failures

## 4. Application layer

- [x] 4.1 Define repository interfaces: `IStoreRepository`, `IProfessionalRepository`, `IServiceRepository`, `IAppointmentRepository`
- [x] 4.2 Add `DependencyInjection` extension stub in Application (empty register method for future handlers)

## 5. Infrastructure layer

- [x] 5.1 Add `InfrastructureServiceCollectionExtensions` with empty `AddInfrastructure()` stub (no DbContext yet)
- [x] 5.2 Ensure Infrastructure project references Application only (plus packages)

## 6. API host

- [x] 6.1 Configure `Program.cs`: controllers, Swagger in Development, `AddApplication()` and `AddInfrastructure()` stubs
- [x] 6.2 Add `GET /health` endpoint returning 200
- [x] 6.3 Add `appsettings.json` and `appsettings.Development.json` placeholders for future Supabase connection

## 7. Documentation and verification

- [x] 7.1 Add README with .NET 8 prerequisite, restore, build, and run instructions
- [x] 7.2 Run `dotnet build` and `dotnet run --project src/HoraMarcada.Api`; confirm `/health` returns 200
