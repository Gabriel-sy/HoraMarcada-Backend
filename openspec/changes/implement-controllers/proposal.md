## Why

The application currently contains a rich domain and application layer with UseCases, but lacks the API controllers to expose these features as HTTP endpoints. Exposing these endpoints is necessary for clients to interact with the backend service.

## What Changes

- Add ASP.NET Core API Controllers under `src/HoraMarcada.Api/Controllers`.
- Structure endpoints such that each has its own dedicated folder and controller class file (e.g., `Service/Create/CreateServiceController.cs` or `Store/Create/CreateStoreController.cs`).
- Introduce FluentValidation to validate HTTP request payloads before invoking UseCases.
- Place validators for each endpoint's request payload within the same endpoint folder (e.g., `Service/Create/CreateServiceRequestValidator.cs`).
- Integrate FluentValidation registration in `Program.cs`.

## Capabilities

### New Capabilities
- `api-endpoints`: Expose existing UseCases via HTTP REST endpoints using individual-controller structure and FluentValidation.

### Modified Capabilities

## Impact

- Adds FluentValidation NuGet packages to `Directory.Packages.props` and `src/HoraMarcada.Api/HoraMarcada.Api.csproj`.
- Populates `src/HoraMarcada.Api/Controllers/` with endpoint folders containing controllers, request/response models, and validators.
- Modifies `src/HoraMarcada.Api/Program.cs` to register FluentValidation validators and configure validation error handling/mapping.
