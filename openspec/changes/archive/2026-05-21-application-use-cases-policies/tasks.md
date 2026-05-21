## 1. Domain entity behavior

- [x] 1.1 Add `Appointment.Cancel()`, `Complete()`, and `MarkCancelTokenUsed()` with invariant checks
- [x] 1.2 Add `Professional.Deactivate()` and `Service.Deactivate()`
- [x] 1.3 Add update helpers on `Store`, `Professional`, and `Service` as needed by use cases

## 2. Application abstractions

- [x] 2.1 Add `NotFoundException`, `ConflictException`, and `ForbiddenException` in Application
- [x] 2.2 Add `ITimeProvider` and `SystemTimeProvider`
- [x] 2.3 Add input models under `Application/Models/` for each use case

## 3. Repository port extensions

- [x] 3.1 Extend `IStoreRepository` with `GetByUserIdAsync` and `UpdateAsync`
- [x] 3.2 Extend `IAppointmentRepository` with list-by-store, list-by-professional-date, get-by-cancel-token, and `UpdateAsync`
- [x] 3.3 Extend `IProfessionalRepository` and `IServiceRepository` with `UpdateAsync`
- [x] 3.4 Implement new methods in EF repositories (`EfStoreRepository`, `EfAppointmentRepository`, etc.)

## 4. Ownership policy

- [x] 4.1 Define `IStoreOwnershipPolicy` in `Application/Policies/`
- [x] 4.2 Implement `StoreOwnershipPolicy` using `IStoreRepository`

## 5. Use case interfaces and implementations

- [x] 5.1 Define and implement `IStoreService` (create, get by user, update)
- [x] 5.2 Define and implement `IProfessionalService` (add, list, update, deactivate)
- [x] 5.3 Define and implement `IServiceOfferingService` (add, list, update, list professionals for service)
- [x] 5.4 Define and implement `IAvailabilityService` per PRD 6.4 algorithm
- [x] 5.5 Define and implement `IAppointmentService` (book, list, cancel by owner, cancel by token, complete)

## 6. Dependency injection and verification

- [x] 6.1 Register all services, policy, and `ITimeProvider` in `AddApplication()`
- [x] 6.2 Run `dotnet build` on the solution and fix any compile errors
