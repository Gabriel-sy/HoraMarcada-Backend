## ADDED Requirements

### Requirement: Book appointment with conflict check

`IAppointmentService` SHALL create an appointment for a store slug or id after validating the store is active, the service and professional belong to the store, and the requested slot is still available. It SHALL persist via `IAppointmentRepository`.

#### Scenario: Successful booking

- **WHEN** a valid booking request is submitted for an available slot
- **THEN** an appointment is created with status confirmed and returned

#### Scenario: Unavailable slot rejected

- **WHEN** booking is attempted for a slot that fails the availability check
- **THEN** the operation fails with a conflict error and no appointment is created

### Requirement: List appointments for entrepreneur panel

`IAppointmentService` SHALL list appointments for a store within a date range, optionally filtered by professional id, after ownership verification.

#### Scenario: Owner lists week appointments

- **WHEN** the store owner requests appointments between start and end dates
- **THEN** all non-deleted appointments in range are returned ordered by start time

### Requirement: Cancel appointment by entrepreneur

`IAppointmentService` SHALL cancel an appointment by id after ownership verification, invoking domain `Cancel()` and persisting.

#### Scenario: Owner cancels appointment

- **WHEN** cancel is called by the store owner for a confirmed appointment
- **THEN** status becomes canceled

### Requirement: Cancel appointment by token

`IAppointmentService` SHALL cancel by cancel token when the token matches, is not used, and the appointment is not already canceled or completed.

#### Scenario: Valid token cancels once

- **WHEN** cancel by token is called with a valid unused token
- **THEN** the appointment is canceled and cancel token is marked used

#### Scenario: Reused token rejected

- **WHEN** cancel by token is called after the token was already used
- **THEN** the operation fails and status is unchanged

### Requirement: Complete appointment

`IAppointmentService` SHALL mark an appointment completed after ownership verification.

#### Scenario: Owner marks completed

- **WHEN** complete is called for a confirmed appointment by the store owner
- **THEN** status becomes completed
