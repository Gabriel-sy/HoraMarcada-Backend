## Purpose

Defines the domain model entities, aggregates, and value objects for the Marque backend.
## Requirements
### Requirement: Store aggregate root

The domain model SHALL include a `Store` entity with: unique identifier, owning `UserId`, `Name`, `Slug`, optional `LogoUrl`, optional `Address` with optional latitude/longitude, required business `WhatsApp`, `Active` flag, and `CreatedAt`.

#### Scenario: Store requires slug and whatsapp

- **WHEN** creating a store without a slug or without a business WhatsApp number
- **THEN** creation fails with a domain validation error

### Requirement: Professional entity

The domain model SHALL include a `Professional` entity belonging to a store with: identifier, `StoreId`, `Name`, optional `PhotoUrl`, optional `WhatsApp`, `Active` flag, and `CreatedAt`.

#### Scenario: Professional belongs to store

- **WHEN** a professional is created with a valid `StoreId`
- **THEN** the professional is associated with that store

### Requirement: Weekly hours entity

The domain model SHALL include `WeeklyHours` per professional with: `DayOfWeek` (0=Sunday through 6=Saturday), `StartTime`, `EndTime`, optional `LunchStart` and `LunchEnd`, and `IntervalMinutes` defaulting to 30.

#### Scenario: Invalid time range rejected

- **WHEN** weekly hours are defined with `StartTime` greater than or equal to `EndTime`
- **THEN** creation fails with a domain validation error

#### Scenario: Lunch interval within work hours

- **WHEN** lunch start and end are provided and lunch falls outside start/end work times
- **THEN** creation fails with a domain validation error

### Requirement: Service entity

The domain model SHALL include a `Service` entity with: identifier, `StoreId`, `Name`, `DurationMinutes`, optional `Price`, `Active` flag, and support for associating multiple professionals (N:N semantics).

#### Scenario: Service requires positive duration

- **WHEN** a service is created with duration less than or equal to zero
- **THEN** creation fails with a domain validation error

### Requirement: Appointment aggregate

The domain model SHALL include an `Appointment` entity with: identifier, `StoreId`, `ProfessionalId`, `ServiceId`, `CustomerName`, `CustomerWhatsApp`, `StartTime`, `EndTime`, `AppointmentStatus`, optional `CancelToken`, `CancelTokenUsed` flag, and `CreatedAt`.

#### Scenario: Appointment status enum

- **WHEN** inspecting appointment status in the domain layer
- **THEN** only `Confirmed`, `Canceled`, and `Completed` values are representable

#### Scenario: End time after start time

- **WHEN** an appointment is created with `EndTime` less than or equal to `StartTime`
- **THEN** creation fails with a domain validation error

### Requirement: Value objects for constrained fields

The domain layer SHALL use value objects for `Slug`, phone/WhatsApp numbers, and money/price where applicable, encapsulating validation rules.

#### Scenario: Invalid slug rejected

- **WHEN** a slug contains characters outside the allowed URL-safe pattern or is empty
- **THEN** slug creation fails with a domain validation error

### Requirement: Repository ports defined in Application layer

The Application layer SHALL define repository interfaces for persisting and loading domain entities (at minimum: stores, professionals, services, appointments), without implementing persistence.

#### Scenario: Application does not reference EF Core

- **WHEN** inspecting `Marque.Application.csproj`
- **THEN** it contains no Entity Framework Core package references

### Requirement: Domain has no external framework dependencies

`Marque.Domain` SHALL not reference Entity Framework, ASP.NET Core, Hangfire, or HTTP client libraries.

#### Scenario: Domain project dependency check

- **WHEN** listing package and project references on `Marque.Domain`
- **THEN** only BCL/analyzer dependencies are present

### Requirement: Appointment state transitions in domain

The `Appointment` entity SHALL expose domain methods to cancel, complete, and mark cancel token used. Transitions SHALL enforce valid state (e.g. cannot cancel an already completed appointment).

#### Scenario: Cancel confirmed appointment

- **WHEN** `Cancel()` is invoked on a confirmed appointment
- **THEN** status becomes `Canceled`

#### Scenario: Cannot cancel completed appointment

- **WHEN** `Cancel()` is invoked on a completed appointment
- **THEN** a domain validation error is thrown

#### Scenario: Complete confirmed appointment

- **WHEN** `Complete()` is invoked on a confirmed appointment
- **THEN** status becomes `Completed`

#### Scenario: Mark cancel token used

- **WHEN** `MarkCancelTokenUsed()` is invoked after a successful token cancellation
- **THEN** `CancelTokenUsed` is true

### Requirement: Soft deactivation for professional and service

`Professional` and `Service` entities SHALL support deactivation via domain methods setting `Active` to false without deleting the record.

#### Scenario: Deactivate professional

- **WHEN** `Deactivate()` is called on an active professional
- **THEN** `Active` is false

#### Scenario: Deactivate service

- **WHEN** `Deactivate()` is called on an active service
- **THEN** `Active` is false

