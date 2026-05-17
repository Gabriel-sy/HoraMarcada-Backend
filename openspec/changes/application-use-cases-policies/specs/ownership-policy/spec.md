## ADDED Requirements

### Requirement: Store ownership policy interface

The Application layer SHALL define `IStoreOwnershipPolicy` with a method to verify that a given user owns a store before store-scoped mutations.

#### Scenario: Policy interface is persistence-agnostic

- **WHEN** inspecting `IStoreOwnershipPolicy`
- **THEN** it depends only on repository abstractions and primitive identifiers, not on HTTP context or JWT types

### Requirement: Ownership verification fails for non-owner

The ownership policy implementation SHALL load the store by id and SHALL fail when `store.UserId` does not equal the supplied `userId` or when the store does not exist.

#### Scenario: Non-owner is rejected

- **WHEN** `EnsureUserOwnsStoreAsync` is called with a user id that does not match the store owner
- **THEN** the call fails with an authorization error and no mutation proceeds

#### Scenario: Missing store is rejected

- **WHEN** `EnsureUserOwnsStoreAsync` is called for a store id that does not exist
- **THEN** the call fails with a not-found or authorization error and no mutation proceeds

### Requirement: Use cases invoke policy before mutations

Store-scoped use cases (professional, service, appointment panel operations, store update) SHALL invoke the ownership policy before persisting changes.

#### Scenario: Update professional checks ownership

- **WHEN** an authenticated user attempts to update a professional belonging to another user's store
- **THEN** the use case fails before calling repository update
