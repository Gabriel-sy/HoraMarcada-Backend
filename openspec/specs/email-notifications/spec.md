# email-notifications Specification

## Purpose
TBD - created by archiving change implement-email-notifications. Update Purpose after archive.
## Requirements
### Requirement: Email data capture
The system SHALL capture and validate email addresses for customers (during booking), stores (during configuration), and professionals (optionally during setup).

#### Scenario: Customer email is required for booking
- **WHEN** a customer attempts to book an appointment with a valid email
- **THEN** the booking is created and the customer's email is saved

#### Scenario: Store email is required for configuration
- **WHEN** an entrepreneur creates or updates a store with a valid email
- **THEN** the store is saved with the notification email

#### Scenario: Professional email is optional
- **WHEN** an entrepreneur adds or updates a professional with a valid email or no email
- **THEN** the professional is saved successfully

### Requirement: Decoupled notification sending
The application layer SHALL use a decoupled notification service abstraction to trigger notification events without depending on the underlying transport implementation.

#### Scenario: Triggering notification during booking
- **WHEN** an appointment is successfully booked
- **THEN** the application invokes the notification service to send booking confirmations

#### Scenario: Triggering notification during cancellation
- **WHEN** an appointment is canceled
- **THEN** the application invokes the notification service to send cancellation notices

### Requirement: Send booking confirmation email via Resend
The infrastructure implementation of the notification service SHALL send email messages using Resend when a booking confirmation event is triggered.

#### Scenario: Sending confirmation email
- **WHEN** a booking confirmation event is triggered for a confirmed appointment
- **THEN** Resend is used to send confirmation emails to the customer, the store, and the professional (if configured)

### Requirement: Send cancellation notice email via Resend
The infrastructure implementation of the notification service SHALL send email messages using Resend when a cancellation event is triggered.

#### Scenario: Sending cancellation email
- **WHEN** a cancellation event is triggered
- **THEN** Resend is used to send cancellation emails to the customer, the store, and the professional (if configured)

