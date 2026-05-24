## 1. Setup & Package Configuration

- [x] 1.1 Add Resend dependency (version 0.5.1) to Directory.Packages.props
- [x] 1.2 Add Resend package reference to HoraMarcada.Infrastructure.csproj

## 2. Domain & DB Schema Updates

- [x] 2.1 Create Email value object in HoraMarcada.Domain/ValueObjects/Email.cs
- [x] 2.2 Add Email property to Store entity and update its factory/update methods
- [x] 2.3 Add optional Email property to Professional entity and update its factory/update methods
- [x] 2.4 Add CustomerEmail property to Appointment entity and update its factory/update methods
- [x] 2.5 Configure EF Core conversions for Email value object in ValueObjectConverters
- [x] 2.6 Configure store, professional, and appointment entities to map new email columns in Infrastructure Configurations
- [x] 2.7 Create a new EF Core database migration for the email columns
- [x] 2.8 Update existing database schemas by running the migration

## 3. Abstraction & Infrastructure Implementation

- [x] 3.1 Define INotificationService interface in HoraMarcada.Application/Common
- [x] 3.2 Implement ResendEmailNotificationService in HoraMarcada.Infrastructure/Notifications
- [x] 3.3 Configure Resend API client and register ResendEmailNotificationService in Infrastructure dependency injection setup

## 4. Integration into Use Cases and Controllers

- [x] 4.1 Update input/output models (BookAppointmentInput, CreateStoreInput, UpdateStoreInput, AddProfessionalInput, UpdateProfessionalInput) to include email fields
- [x] 4.2 Update request validators (BookAppointmentRequestValidator, CreateStoreRequestValidator, etc.) to validate email fields
- [x] 4.3 Inject INotificationService into BookAppointmentUseCase and dispatch booking confirmation notifications
- [x] 4.4 Inject INotificationService into CancelAppointmentUseCase and CancelAppointmentByTokenUseCase and dispatch cancellation notifications
- [x] 4.5 Update controllers to accept the new email parameters in request bodies

## 5. Verification & Testing

- [x] 5.1 Add unit tests for the Email value object validation
- [x] 5.2 Add unit tests for ResendEmailNotificationService mapping and behavior
- [x] 5.3 Add unit tests verifying that BookAppointmentUseCase and cancellation use cases call INotificationService
