## Context

The backend application contains all the business logic, entity models, and UseCases required for stores, professionals, services, availability, and appointments scheduling. However, it lacks a presentation layer (HTTP REST API). This change introduces ASP.NET Core 8 Controllers in the `HoraMarcada.Api` project to expose these UseCases to clients.

## Goals / Non-Goals

**Goals:**
- Expose all existing UseCases via standard, RESTful HTTP endpoints.
- Structure controllers with one folder and file per endpoint (single-action controllers) to ensure high cohesion and readability.
- Integrate FluentValidation for request payload validation with validators co-located with their respective controllers.
- Map custom domain/application exceptions (e.g., `NotFoundException`, `ConflictException`, `DomainValidationException`) to corresponding HTTP status codes automatically.

**Non-Goals:**
- Implement JWT authentication or Supabase auth validation logic (which is noted as private in the PRD, but out of scope for the simple controller mapping task unless required for routing).

## Decisions

### 1. Single-Endpoint Controller Pattern
We will use a single-action controller pattern, where each controller handles exactly one HTTP endpoint and is placed in its own folder under `src/HoraMarcada.Api/Controllers/`.
- *Rationale*: This matches the user's requirement of "one folder and file for each endpoint". It keeps controllers small, focused, and easy to maintain.
- *Naming Convention*: `[Verb][Action]Controller.cs` (e.g., `CreateServiceController.cs`) containing a single action method decorated with the appropriate HTTP verb attribute.

### 2. Route Registry & Endpoint Mapping
Here is the mapping from endpoints to UseCases:

| Endpoint | Verb | Controller | UseCase |
| --- | --- | --- | --- |
| `/api/stores` | POST | `CreateStoreController` | `CreateStoreUseCase` |
| `/api/stores/user/{userId}` | GET | `GetStoreByUserIdController` | `GetStoreByUserIdUseCase` |
| `/api/stores/{storeId}` | PUT | `UpdateStoreController` | `UpdateStoreUseCase` |
| `/api/stores/{storeId}/professionals` | POST | `AddProfessionalController` | `AddProfessionalUseCase` |
| `/api/stores/{storeId}/professionals` | GET | `ListProfessionalsController` | `ListProfessionalsUseCase` |
| `/api/stores/{storeId}/professionals/{professionalId}` | PUT | `UpdateProfessionalController` | `UpdateProfessionalUseCase` |
| `/api/stores/{storeId}/professionals/{professionalId}/deactivate` | POST | `DeactivateProfessionalController` | `DeactivateProfessionalUseCase` |
| `/api/stores/{storeId}/services` | POST | `AddServiceController` | `AddServiceUseCase` |
| `/api/stores/{storeId}/services` | GET | `ListServicesController` | `ListServicesUseCase` |
| `/api/stores/{storeId}/services/{serviceId}` | PUT | `UpdateServiceController` | `UpdateServiceUseCase` |
| `/api/stores/{storeId}/services/{serviceId}/professionals` | GET | `ListProfessionalsForServiceController` | `ListProfessionalsForServiceUseCase` |
| `/api/stores/{storeId}/availability` | GET | `GetAvailableSlotsController` | `GetAvailableSlotsUseCase` |
| `/api/appointments` | POST | `BookAppointmentController` | `BookAppointmentUseCase` |
| `/api/appointments` | GET | `ListAppointmentsController` | `ListAppointmentsUseCase` |
| `/api/appointments/{appointmentId}/cancel` | POST | `CancelAppointmentController` | `CancelAppointmentUseCase` |
| `/api/appointments/cancel-by-token` | POST | `CancelAppointmentByTokenController` | `CancelAppointmentByTokenUseCase` |
| `/api/appointments/{appointmentId}/complete` | POST | `CompleteAppointmentController` | `CompleteAppointmentUseCase` |

*Note: For `DeactivateProfessional`, `CancelAppointment`, and `CompleteAppointment`, we use POST endpoints since they are state-changing operations that represent specific workflow commands.*

### 3. Request Validation with FluentValidation
- We will add FluentValidation packages (`FluentValidation` and `FluentValidation.DependencyInjectionExtensions`) to `Directory.Packages.props` and `HoraMarcada.Api.csproj`.
- Each controller requiring input validation will have a request DTO, and a corresponding `IValidator<TRequest>` co-located in the same folder.
- Validation will be executed manually inside the controller's action by injecting the corresponding `IValidator<TRequest>` and calling `ValidateAsync`. If validation fails, we will return `BadRequest(validationResult.ToDictionary())` or use `ModelState.AddModelError` to return a standardized Problem Details response via `ValidationProblem(ModelState)`.
- *Rationale*: Explicit validation is easier to debug, customize, and doesn't rely on global magic. Co-locating request DTOs, validators, and controllers ensures self-containment.

### 4. Global Exception Filter for Exception Mapping
We will implement an `ExceptionFilterAttribute` (named `ApiExceptionFilterAttribute`) registered globally in `Program.cs`.
- *Rationale*: This avoids boilerplate try-catch blocks in each controller and guarantees consistent error responses.
- *Mappings*:
  - `NotFoundException` -> HTTP 404 (Not Found)
  - `ConflictException` -> HTTP 409 (Conflict)
  - `ForbiddenException` -> HTTP 403 (Forbidden)
  - `DomainValidationException` -> HTTP 400 (Bad Request)

## Risks / Trade-offs

- **[Risk] Multiple project packages duplication** -> *Mitigation*: Register the packages in `Directory.Packages.props` and import them cleanly into the API project.
- **[Risk] High number of files** -> *Mitigation*: The project structure is organized by business aggregate (`Stores`, `Professionals`, `Services`, `Availability`, `Appointments`) and then endpoint subfolders, making the directory easy to navigate.
