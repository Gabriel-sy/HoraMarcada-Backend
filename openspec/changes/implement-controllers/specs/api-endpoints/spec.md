## ADDED Requirements

### Requirement: Single-endpoint Controller Structure

The API controllers SHALL be structured with one folder and file for each endpoint under `src/Marque.Api/Controllers/`. The namespace of each controller MUST match its folder structure (e.g., `Marque.Api.Controllers.Services.AddService`).

#### Scenario: Dedicated endpoint folders
- **WHEN** inspecting `src/Marque.Api/Controllers`
- **THEN** each endpoint resides in its own subfolder containing a single controller class file (e.g., `src/Marque.Api/Controllers/Services/Add/AddServiceController.cs`)

### Requirement: Request Validation with FluentValidation

All request payloads that modify data or have business constraints SHALL be validated using FluentValidation before passing inputs to UseCases. The validator files MUST reside in the same folder as their corresponding controller.

#### Scenario: Request validation fails
- **WHEN** a client sends a request that violates FluentValidation rules
- **THEN** the API returns HTTP 400 Bad Request containing validation error details

#### Scenario: Request validation passes
- **WHEN** a client sends a request that satisfies all FluentValidation rules
- **THEN** the request is successfully processed and forwarded to the UseCase

### Requirement: Exception Mapping to HTTP Status Codes

The API SHALL catch and map application and domain exceptions to appropriate HTTP status codes:
- `NotFoundException` -> HTTP 404 Not Found
- `ConflictException` -> HTTP 409 Conflict
- `ForbiddenException` -> HTTP 403 Forbidden
- `DomainValidationException` -> HTTP 400 Bad Request

#### Scenario: NotFoundException throws 404
- **WHEN** a UseCase execution results in a `NotFoundException`
- **THEN** the API returns HTTP 404 Not Found with the exception message

#### Scenario: ConflictException throws 409
- **WHEN** a UseCase execution results in a `ConflictException`
- **THEN** the API returns HTTP 409 Conflict with the exception message

#### Scenario: DomainValidationException throws 400
- **WHEN** a UseCase execution results in a `DomainValidationException`
- **THEN** the API returns HTTP 400 Bad Request with the exception message
