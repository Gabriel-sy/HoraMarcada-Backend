# HoraMarcada Backend

ASP.NET Core 8 backend for the HoraMarcada appointment scheduling platform.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

## Build and run

```bash
dotnet restore
dotnet build
dotnet run --project src/HoraMarcada.Api
```

The API listens on the URLs shown in the console (see `src/HoraMarcada.Api/Properties/launchSettings.json`). Verify the host with:

```bash
curl http://localhost:5213/health
```

Expected response: HTTP 200 with `{"status":"healthy"}`.

## Solution structure

| Project | Role |
|---------|------|
| `HoraMarcada.Domain` | Entities, value objects, domain rules |
| `HoraMarcada.Application` | Repository ports, use-case orchestration |
| `HoraMarcada.Infrastructure` | EF Core, external integrations (stubs in foundation) |
| `HoraMarcada.Api` | HTTP host |

Dependency flow: `Api` → `Infrastructure` + `Application` → `Domain`.

## Configuration

### Supabase / PostgreSQL

The API uses Entity Framework Core with PostgreSQL via `ConnectionStrings:Supabase`.

**Local development** — set the connection string in `src/HoraMarcada.Api/appsettings.Development.json`, or override with user secrets:

```bash
dotnet user-secrets set "ConnectionStrings:Supabase" "Host=...;Port=5432;Database=...;Username=...;Password=..." --project src/HoraMarcada.Api
```

For Supabase, copy the connection string from **Project Settings → Database**. Use the **direct** connection (port 5432) for `dotnet ef database update`; the pooler URL is fine for API runtime.

Environment variable override: `ConnectionStrings__Supabase`.

### EF Core migrations

Requires [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) and the EF CLI:

```bash
dotnet tool install --global dotnet-ef --version 8.0.11
```

Create a new migration:

```bash
dotnet ef migrations add <Name> --project src/HoraMarcada.Infrastructure --startup-project src/HoraMarcada.Api
```

Apply migrations to the database:

```bash
dotnet ef database update --project src/HoraMarcada.Infrastructure --startup-project src/HoraMarcada.Api
```

After applying schema migrations, run `scripts/supabase-rls.sql` manually in the Supabase SQL editor to enable Row Level Security policies (not managed by EF).

JWT and Hangfire are referenced but not wired yet.
