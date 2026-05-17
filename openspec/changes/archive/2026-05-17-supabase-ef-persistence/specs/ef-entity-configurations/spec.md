## ADDED Requirements

### Requirement: Snake_case table and column names

Entity type configurations SHALL map domain entities to PostgreSQL tables and columns using snake_case names matching PRD section 5.2 (`stores`, `professionals`, `weekly_hours`, `services`, `service_professional`, `appointments`).

#### Scenario: Store table mapping

- **WHEN** the initial migration is generated for `Store`
- **THEN** the table is named `stores` with columns including `user_id`, `logo_url`, `address_lat`, `address_lng`, and `created_at`

### Requirement: Relationships and constraints per PRD

Configurations SHALL define foreign keys, cascade deletes, and unique constraints as specified in the PRD: unique `stores.slug`, unique `(professional_id, day_of_week)` on weekly hours, composite primary key on `service_professional`, unique `appointments.cancel_token`, and check constraint on `appointments.status`.

#### Scenario: Weekly hours unique per day

- **WHEN** two weekly hour rows share the same professional and day of week
- **THEN** the database rejects the insert via unique constraint

### Requirement: Value object converters

EF configurations SHALL persist `Slug`, `PhoneNumber`, and `Money` using appropriate PostgreSQL column types (`text` or `numeric`) via value converters without exposing primitive types on domain entities.

#### Scenario: Slug stored as text

- **WHEN** a store with slug `my-barbershop` is saved
- **THEN** the `stores.slug` column contains the string `my-barbershop`

### Requirement: Appointment indexes

The configuration or migration SHALL create indexes `idx_appointments_professional_start` and `idx_appointments_store_start` on `(professional_id, start_time)` and `(store_id, start_time)` respectively.

#### Scenario: Index names match PRD

- **WHEN** inspecting the database after migration apply
- **THEN** both appointment indexes exist with the PRD-specified column sets
