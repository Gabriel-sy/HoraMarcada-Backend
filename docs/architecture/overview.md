# Architecture

HoraMarcada follows Clean Architecture with four source projects and one test project.

## Solution layout

```
HoraMarcada.sln
├── src/HoraMarcada.Domain          # Entities, value objects, domain exceptions
├── src/HoraMarcada.Application     # Use cases, repository ports, policies, app services
├── src/HoraMarcada.Infrastructure  # EF Core, WAHA, Hangfire, configuration
├── src/HoraMarcada.Api             # ASP.NET Core host, controllers, validators
└── tests/HoraMarcada.UnitTests     # xUnit tests
```

Dependency flow: `Api → Infrastructure + Application → Domain`. Infrastructure also references Application to implement ports.

Central package versions: `Directory.Packages.props`.

## Host and pipeline

Entry point: `src/HoraMarcada.Api/Program.cs`

1. Registers CORS (`AllowAll`), controllers, ProblemDetails, `GlobalExceptionHandler`, FluentValidation, Swagger (JWT Bearer scheme).
2. Calls `AddApplication()` and `AddInfrastructure(configuration)`.
3. Middleware: exception handler → Swagger (dev) → CORS → authentication → authorization → Hangfire dashboard → recurring jobs → controllers.

Port **5213** (also in `launchSettings.json` and `Dockerfile`).

### Exception mapping

`GlobalExceptionHandler` maps domain/application exceptions to ProblemDetails:

| Exception | HTTP status |
|-----------|---------------|
| `NotFoundException` | 404 |
| `ConflictException` | 409 |
| `ForbiddenException` | 403 |
| `DomainValidationException` | 400 |

## Dependency injection

### Application (`ApplicationServiceCollectionExtensions`)

Registers use cases for stores, professionals, services, time blocks, appointments, customers, and availability.

Shared services:

| Interface | Implementation |
|-----------|----------------|
| `ITimeProvider` | `SystemTimeProvider` |
| `ICancelTokenGenerator` | `CancelTokenGenerator` |
| `IStoreOwnershipPolicy` | `StoreOwnershipPolicy` |
| `IAppointmentNotificationService` | `AppointmentNotificationService` |
| `IAvailabilityService` | `AvailabilityService` |

### Infrastructure (`InfrastructureServiceCollectionExtensions`)

- `HoraMarcadaDbContext` with Npgsql
- EF repository implementations (`Ef*Repository`)
- `IAppConfiguration` → `AppConfiguration`
- Notification stack: queue decorator → WAHA dispatcher
- Hangfire with PostgreSQL storage (same connection string)
- JWT Bearer authentication against Supabase Auth

## Authentication

No Supabase SDK in the API. Entrepreneurs authenticate with Supabase-issued JWTs validated as Bearer tokens.

Configuration:

- `Supabase:Url` → JWT authority `{url}/auth/v1`
- Audience: `authenticated`

User ID is extracted from `ClaimTypes.NameIdentifier` or `sub` (`ClaimsPrincipalExtensions`). Controllers marked `[Authorize]` require a valid JWT.

**Authorization model:**

- Protected endpoints: store management, professionals, services, time blocks, appointment panel, customers.
- Public endpoints: booking, token-based cancel/confirm, store-by-slug, availability, public service/professional lists.

Store ownership is enforced in use cases via `IStoreOwnershipPolicy` (`store.UserId == authenticatedUserId`). Mismatches return 404, not 403.

`scripts/supabase-rls.sql` defines RLS for direct Supabase/PostgREST access. The API uses a service connection string and enforces ownership in the application layer.

## Persistence

- **ORM:** Entity Framework Core 8 with PostgreSQL (Npgsql)
- **DbContext:** `HoraMarcada.Infrastructure/Persistence/HoraMarcadaDbContext.cs`
- **Configurations:** `Persistence/Configurations/*.cs`
- **Migrations:** `Infrastructure/Migrations/` (apply manually; no auto-migrate on startup)
- **Design-time factory:** reads Api `appsettings.json` for connection string

Repository pattern: interfaces in `Application/Repositories/`, implementations in `Infrastructure/Persistence/Repositories/`.

## Background processing

### Hangfire

- Storage: PostgreSQL (same `ConnectionStrings:Supabase`)
- Dashboard: `/hangfire` (no authorization filter configured)
- **Recurring:** `SendDailySummaryJob` — cron `0 8 * * *`, timezone `America/Sao_Paulo`
- **Deferred:** `SendReminderJob` — scheduled per appointment at booking time via `HangfireBackgroundJobService`

### In-process notification queue

`NotificationQueueProcessor` (hosted service) processes WhatsApp send jobs with retries. Most notification types go through `QueueNotificationServiceDecorator`; daily summary bypasses the queue. See [WhatsApp integration](../integrations/whatsapp.md).

## API design

Controllers follow **one controller per action** under feature folders:

```
Controllers/Stores/Create/CreateStoreController.cs
Controllers/Stores/Create/CreateStoreRequestValidator.cs
```

Routes are prefixed with `api/`. Request validation uses FluentValidation in the same folder as the controller.

## Docker

`Dockerfile`: multi-stage build (`sdk:8.0` → `aspnet:8.0`), exposes port 5213.

`docker-compose.yml` services:

| Service | Role |
|---------|------|
| `backend` | HoraMarcada API |
| `waha` | WAHA WhatsApp gateway (`devlikeapro/waha:latest`) |

Compose sets `Waha__BaseUrl` and `Waha__ApiKey` but does **not** include database or Supabase auth config — inject those externally. `NOTIFICATION_CHANNEL` in compose is unused by the C# code.

## Change guidance

| Change type | Start here | Also check |
|-------------|------------|------------|
| New endpoint | `Api/Controllers/`, matching use case | Validator, OpenSpec if spec-driven |
| New business rule | Domain entity or use case | Unit tests in mirrored test folder |
| Schema change | EF configuration + migration | `scripts/supabase-rls.sql` if RLS affected |
| New notification event | `INotificationService`, WAHA templates | `AppointmentNotificationService`, queue tests |
| Auth/ownership | `StoreOwnershipPolicy`, controller `[Authorize]` | JWT config in Infrastructure DI |
