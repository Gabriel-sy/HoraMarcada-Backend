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

`appsettings.Development.json` includes a placeholder `ConnectionStrings:Supabase` for future PostgreSQL (Supabase) integration. JWT, Hangfire, and EF Core are referenced but not wired in the foundation change.
