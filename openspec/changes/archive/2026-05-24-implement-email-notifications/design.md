## Context

Initially, the application was designed to send notifications to customers, professionals, and store owners via WhatsApp. However, native WhatsApp integrations present cost and setup barriers. We are transitioning notifications to email using the Resend service. To ensure we can easily swap notifications to another channel (WhatsApp, SMS, or Telegram) later, we must design a highly decoupled abstraction using the Dependency Inversion Principle (DIP).

## Goals / Non-Goals

**Goals:**
- Add email fields to stores, professionals, and appointments.
- Define a channel-agnostic `INotificationService` interface in the Application layer.
- Implement `ResendEmailNotificationService` using the official `Resend` SDK (version `0.5.1`) in the Infrastructure layer.
- Integrate notification dispatches into `BookAppointmentUseCase`, `CancelAppointmentUseCase`, and `CancelAppointmentByTokenUseCase`.
- Add integration and unit tests for the email notification logic.

**Non-Goals:**
- Implementing actual WhatsApp notifications in this change.
- Storing multiple contact emails per professional or store.

## Decisions

### Decision 1: Channel-Agnostic Notification Interface
- **Choice**: Define `INotificationService` with methods representing business events (e.g. `SendBookingConfirmationAsync`), rather than transport methods (e.g. `SendEmailAsync`).
- **Rationale**: Keeps use cases completely decoupled from the notification medium. Changing from email to WhatsApp in the future only requires implementing a new `INotificationService` adapter in Infrastructure, with zero use case modifications.

### Decision 2: Official Resend .NET SDK
- **Choice**: Use the official `Resend` package version `0.5.1`.
- **Rationale**: Standard library with built-in ASP.NET dependency injection support (`AddResend`), minimizing boilerplate.

### Decision 3: DB and Aggregate Changes for Emails
- **Choice**: Store emails directly on `Store`, `Professional`, and `Appointment` entities.
- **Rationale**: Required so the notification system knows who to email. To avoid breaking existing fields, we will preserve the existing `WhatsApp` phone number columns and add new `Email` columns.

## Risks / Trade-offs

- **Risk**: Customers typing incorrect emails.
- **Mitigation**: Add strict email validation in `BookAppointmentRequestValidator` using FluentValidation.
