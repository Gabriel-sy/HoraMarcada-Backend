# Marque Backend

ASP.NET Core 8 backend for the Marque appointment scheduling platform.

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

## Build and run

```bash
dotnet restore
dotnet build
dotnet run --project src/Marque.Api
```

The API listens on the URLs shown in the console (see `src/Marque.Api/Properties/launchSettings.json`). Verify the host with:

```bash
curl http://localhost:5213/health
```

Expected response: HTTP 200 with `{"status":"healthy"}`.

## Solution structure

| Project | Role |
|---------|------|
| `Marque.Domain` | Entities, value objects, domain rules |
| `Marque.Application` | Repository ports, use-case orchestration |
| `Marque.Infrastructure` | EF Core, external integrations (stubs in foundation) |
| `Marque.Api` | HTTP host |

Dependency flow: `Api` → `Infrastructure` + `Application` → `Domain`.

## Configuration

### Supabase / PostgreSQL

The API uses Entity Framework Core with PostgreSQL via `ConnectionStrings:Supabase`.

**Local development** — set the connection string in `src/Marque.Api/appsettings.Development.json`, or override with user secrets:

```bash
dotnet user-secrets set "ConnectionStrings:Supabase" "Host=...;Port=5432;Database=...;Username=...;Password=..." --project src/Marque.Api
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
dotnet ef migrations add <Name> --project src/Marque.Infrastructure --startup-project src/Marque.Api
```

Apply migrations to the database:

```bash
dotnet ef database update --project src/Marque.Infrastructure --startup-project src/Marque.Api
```

After applying schema migrations, run `scripts/supabase-rls.sql` manually in the Supabase SQL editor to enable Row Level Security policies (not managed by EF).

JWT and Hangfire are referenced but not wired yet.
