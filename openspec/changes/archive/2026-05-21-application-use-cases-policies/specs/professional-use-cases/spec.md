## ADDED Requirements

### Requirement: Add professional to store

`IProfessionalService` SHALL add a professional to a store after ownership verification, using the domain factory and optional weekly hours.

#### Scenario: Owner adds professional

- **WHEN** the store owner submits a valid professional with weekly hours
- **THEN** the professional is persisted and associated with the store

### Requirement: List professionals by store

`IProfessionalService` SHALL list professionals for a store id, optionally filtering to active only for public-facing callers.

#### Scenario: List active professionals

- **WHEN** list is called with active-only filter for a store
- **THEN** only professionals with `Active == true` are returned

### Requirement: Update professional

`IProfessionalService` SHALL update professional details and weekly hours after ownership verification.

#### Scenario: Owner updates weekly hours

- **WHEN** valid weekly hours are submitted for an owned professional
- **THEN** hours are replaced or merged per design and persisted

### Requirement: Deactivate professional

`IProfessionalService` SHALL deactivate a professional (soft delete) rather than hard-delete, per PRD MVP.

#### Scenario: Deactivate sets inactive flag

- **WHEN** deactivate is called for an owned professional
- **THEN** `Active` is false and the record remains for history
