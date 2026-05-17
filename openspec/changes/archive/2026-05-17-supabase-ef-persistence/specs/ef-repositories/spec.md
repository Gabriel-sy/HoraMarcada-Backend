## ADDED Requirements

### Requirement: EF repository implementations

The Infrastructure layer SHALL provide EF Core implementations for `IStoreRepository`, `IProfessionalRepository`, `IServiceRepository`, and `IAppointmentRepository` registered in `AddInfrastructure()`.

#### Scenario: Repositories registered in DI

- **WHEN** the API host calls `AddInfrastructure()`
- **THEN** resolving `IStoreRepository` returns the EF implementation backed by `MarqueDbContext`

### Requirement: Store repository operations

`IStoreRepository` implementation SHALL support get by id, get by slug, and add with persistence to the database.

#### Scenario: Store persisted after add

- **WHEN** `AddAsync` is called for a new store and changes are saved
- **THEN** a subsequent `GetByIdAsync` returns the same store with matching id and slug

### Requirement: Professional and service list by store

`IProfessionalRepository` and `IServiceRepository` implementations SHALL support get by id, list by store id, and add with persistence.

#### Scenario: List professionals for store

- **WHEN** multiple professionals exist for a store id
- **THEN** `ListByStoreIdAsync` returns all professionals for that store

### Requirement: Professional weekly hours loaded

When loading a professional by id, the repository SHALL include weekly hours schedules where required for domain use.

#### Scenario: Professional includes weekly hours

- **WHEN** a professional with weekly hours is saved and loaded by id
- **THEN** the returned entity exposes the configured weekly schedules

### Requirement: Service professional associations loaded

When loading a service by id, the repository SHALL populate `ProfessionalIds` from the `service_professional` join table.

#### Scenario: Service includes assigned professionals

- **WHEN** a service has professionals assigned via the domain `AssignProfessional` method and is persisted
- **THEN** loading the service by id returns the correct professional id collection

### Requirement: Appointment repository operations

`IAppointmentRepository` implementation SHALL support get by id and add with persistence.

#### Scenario: Appointment persisted

- **WHEN** a valid appointment is added and saved
- **THEN** `GetByIdAsync` returns the appointment with matching times and status
