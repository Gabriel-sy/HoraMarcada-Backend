## ADDED Requirements

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
