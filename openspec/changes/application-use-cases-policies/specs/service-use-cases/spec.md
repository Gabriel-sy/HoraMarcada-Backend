## ADDED Requirements

### Requirement: Add service to store

`IServiceOfferingService` (or equivalent catalog interface) SHALL add a service to a store after ownership verification, with duration, optional price, and optional professional assignments.

#### Scenario: Owner adds service with professionals

- **WHEN** a valid service is created with professional ids that belong to the same store
- **THEN** the service is persisted with N:N professional links

#### Scenario: Professional from another store rejected

- **WHEN** a professional id not belonging to the store is assigned
- **THEN** the operation fails with a validation error

### Requirement: List services by store

The service catalog use case SHALL list services for a store, supporting active-only filter for public booking.

#### Scenario: Public list returns active services

- **WHEN** list is called for public booking with active-only
- **THEN** only active services for that store are returned

### Requirement: Update service

The service catalog use case SHALL update name, duration, price, active flag, and professional assignments after ownership verification.

#### Scenario: Owner updates service duration

- **WHEN** a valid duration change is submitted for an owned service
- **THEN** the service is updated and persisted

### Requirement: List professionals for a service

The service catalog use case SHALL return professionals linked to a given service id for the public booking step 2.

#### Scenario: Professionals for service

- **WHEN** list professionals for service is called for a valid service in an active store
- **THEN** only professionals assigned to that service and active are returned
