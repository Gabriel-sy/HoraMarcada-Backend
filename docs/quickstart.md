# HoraMarcada Backend — Quickstart

HoraMarcada is an appointment scheduling SaaS for small service businesses (barbershops, salons, etc.). Entrepreneurs manage stores, professionals, and services via an authenticated panel. Customers book through a public page without logging in and receive WhatsApp confirmations, reminders, and cancellation links.

This repository is the **ASP.NET Core 8** backend API. Product requirements live in [`PRD.md`](../PRD.md) at the repo root.

## Documentation map

| Topic | Page |
|-------|------|
| Solution layout, auth, persistence, jobs | [Architecture](architecture/overview.md) |
| Entities and business invariants | [Domain model](domain/model.md) |
| Booking, availability, cancellations | [Booking & scheduling](workflows/booking-and-scheduling.md) |
| HTTP endpoints | [API endpoints](api/endpoints.md) |
| WhatsApp notifications (WAHA) | [WhatsApp integration](integrations/whatsapp.md) |

Additional references:

- [OpenSpec](../openspec/) — formal change specs and archived design decisions
- [`scripts/supabase-rls.sql`](../scripts/supabase-rls.sql) — Row Level Security policies for direct Supabase access

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- PostgreSQL (Supabase project)
- Supabase Auth (JWT issuer for entrepreneur endpoints)
- WAHA server for WhatsApp (see [WhatsApp integration](integrations/whatsapp.md))

## Build and run

```bash
dotnet restore
dotnet build
dotnet run --project src/HoraMarcada.Api
```

Default URL: `http://localhost:5213` (see `src/HoraMarcada.Api/Properties/launchSettings.json`).

Swagger UI is available in Development at `/swagger`.

> **Note:** The root [`README.md`](../README.md) mentions a `/health` endpoint and states that JWT/Hangfire are not wired. Both are outdated — JWT auth and Hangfire are configured in `Program.cs`; there is no health check endpoint in the codebase.

## Configuration

Set values in `src/HoraMarcada.Api/appsettings.Development.json`, user secrets, or environment variables. **Do not commit live credentials.**

| Key | Purpose |
|-----|---------|
| `ConnectionStrings:Supabase` | PostgreSQL connection (EF Core + Hangfire storage) |
| `Supabase:Url` | Supabase project URL; JWT authority is `{url}/auth/v1` |
| `MINIMUM_SLOT_LEAD_TIME_MINUTES` | Minimum minutes before a slot can be booked (default `0`) |
| `NOTIFICATIONS_ENABLED` | Kill switch for outbound WhatsApp (default enabled) |
| `Waha:BaseUrl` | WAHA HTTP API base URL |
| `Waha:ApiKey` | WAHA API key (`X-Api-Key` header) |
| `Waha:SessionName` | WAHA session identifier |
| `Waha:HumanBehavior:*` | Typing/delay simulation for WhatsApp sends |

Environment variable overrides use double underscores, e.g. `ConnectionStrings__Supabase`.

```bash
dotnet user-secrets set "ConnectionStrings:Supabase" "Host=...;Port=5432;..." --project src/HoraMarcada.Api
```

`FrontendUrl` is read at runtime for cancel/reschedule links in WhatsApp messages. It is not in appsettings; the code defaults to `https://horamarcada.site`.

## Database migrations

Install EF CLI (once):

```bash
dotnet tool install --global dotnet-ef --version 8.0.11
```

Apply schema:

```bash
dotnet ef database update --project src/HoraMarcada.Infrastructure --startup-project src/HoraMarcada.Api
```

After migrations, run `scripts/supabase-rls.sql` in the Supabase SQL editor for RLS policies on direct database access.

## Docker

`docker-compose.yml` runs the API and a WAHA container. You still need to inject `ConnectionStrings__Supabase`, `Supabase__Url`, and other config via environment or mounted settings. See [Architecture — Docker](architecture/overview.md#docker).

## Tests

```bash
dotnet test
```

Tests live in `tests/HoraMarcada.UnitTests` (xUnit + Moq). Naming convention: `[Method]_When[Condition]_Should[Result]`. Mirror use-case folders when adding tests.

Key areas to run when changing behavior:

- `UseCases/Appointments/` — booking, cancel, notifications orchestration
- `Application/Common/AvailabilityServiceTests.cs` — slot calculation
- `Infrastructure/Notifications/` — WAHA queue and formatting

## Where to change what

| Task | Location |
|------|----------|
| Domain rules and entities | `src/HoraMarcada.Domain/` |
| Use cases and ports | `src/HoraMarcada.Application/UseCases/`, `Repositories/`, `Common/Interfaces/` |
| EF repositories and external services | `src/HoraMarcada.Infrastructure/` |
| HTTP endpoints and request validation | `src/HoraMarcada.Api/Controllers/` (one controller per action) |
| Unit tests | `tests/HoraMarcada.UnitTests/` |

## Agent notes

- Controllers are thin; business logic belongs in use cases under `Application`.
- Entrepreneur mutations enforce store ownership via `IStoreOwnershipPolicy` — returns 404 when the authenticated user does not own the store.
- Notifications are WhatsApp-only via WAHA; email was removed. See [WhatsApp integration](integrations/whatsapp.md).
- `PRD.md` describes the product vision but may drift from implementation (e.g. separate `customers` table, `ConfirmedByUser` status). Prefer source code and this wiki for current behavior.
