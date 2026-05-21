# store-use-cases Specification

## Purpose
TBD - created by archiving change application-use-cases-policies. Update Purpose after archive.
## Requirements
### Requirement: Create store for authenticated user

`IStoreService` SHALL provide a method to create a store bound to the authenticated user's id with name, slug, WhatsApp, and optional logo, address, and coordinates. Creation SHALL use the `Store` domain factory and persist via `IStoreRepository`.

#### Scenario: Successful store creation

- **WHEN** a valid create-store request is submitted for a user with no conflicting slug
- **THEN** a new store is persisted with the user's id as owner and a generated store id is returned

#### Scenario: Duplicate slug rejected

- **WHEN** create store is called with a slug that already exists
- **THEN** the operation fails with a conflict error and no store is created

### Requirement: Get store for authenticated user

`IStoreService` SHALL return the store owned by the authenticated user, or fail when none exists.

#### Scenario: Owner retrieves their store

- **WHEN** get store is called for a user who owns a store
- **THEN** the store aggregate data for that user is returned

### Requirement: Update store with ownership check

`IStoreService` SHALL update mutable store fields after verifying ownership via `IStoreOwnershipPolicy`.

#### Scenario: Owner updates store name

- **WHEN** the store owner submits a valid update
- **THEN** the store is updated and persisted

#### Scenario: Non-owner cannot update

- **WHEN** update store is called by a user who does not own the store
- **THEN** the operation fails before persistence

