## Why

The original design of the system called for booking and cancellation notifications to be sent via WhatsApp. However, because native WhatsApp integrations are not currently viable (due to cost, verification, or API setup complexity), the application will use email notifications via Resend. The notification system must be designed following the Dependency Inversion Principle (DIP) to allow future swaps (e.g., to WhatsApp, SMS, or Telegram) with zero changes to application use cases.

## What Changes

- Add email configurations and fields to support sending emails:
  - Add `CustomerEmail` to `Appointment` entity, database schema, use cases, and request validators.
  - Add `Email` to `Store` entity, database schema, use cases, and request validators.
  - Add optional `Email` to `Professional` entity, database schema, use cases, and request validators.
- Create a decoupled `INotificationService` abstraction in the `HoraMarcada.Application` layer.
- Add the official `Resend` SDK dependency to the solution.
- Implement `ResendEmailNotificationService` in the `HoraMarcada.Infrastructure` layer.
- Integrate email notifications into booking and cancellation use cases.
- Write unit tests verifying that notifications are triggered correctly upon booking and cancellations.

## Capabilities

### New Capabilities
- `email-notifications`: Sends transactional booking confirmation and cancellation emails to customers, stores, and professionals using Resend, driven by a generic application-level notification interface.

### Modified Capabilities
