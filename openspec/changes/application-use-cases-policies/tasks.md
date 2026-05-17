## 1. Domain entity behavior

- [ ] 1.1 Add `Appointment.Cancel()`, `Complete()`, and `MarkCancelTokenUsed()` with invariant checks
- [ ] 1.2 Add `Professional.Deactivate()` and `Service.Deactivate()`
- [ ] 1.3 Add update helpers on `Store`, `Professional`, and `Service` as needed by use cases

## 2. Application abstractions

- [ ] 2.1 Add `NotFoundException`, `ConflictException`, and `ForbiddenException` in Application
- [ ] 2.2 Add `ITimeProvider` and `SystemTimeProvider`
- [ ] 2.3 Add input models under `Application/Models/` for each use case

## 3. Repository port extensions

- [ ] 3.1 Extend `IStoreRepository` with `GetByUserIdAsync` and `UpdateAsync`
- [ ] 3.2 Extend `IAppointmentRepository` with list-by-store, list-by-professional-date, get-by-cancel-token, and `UpdateAsync`
- [ ] 3.3 Extend `IProfessionalRepository` and `IServiceRepository` with `UpdateAsync`
- [ ] 3.4 Implement new methods in EF repositories (`EfStoreRepository`, `EfAppointmentRepository`, etc.)

## 4. Ownership policy

- [ ] 4.1 Define `IStoreOwnershipPolicy` in `Application/Policies/`
- [ ] 4.2 Implement `StoreOwnershipPolicy` using `IStoreRepository`

## 5. Use case interfaces and implementations

- [ ] 5.1 Define and implement `IStoreService` (create, get by user, update)
- [ ] 5.2 Define and implement `IProfessionalService` (add, list, update, deactivate)
- [ ] 5.3 Define and implement `IServiceOfferingService` (add, list, update, list professionals for service)
- [ ] 5.4 Define and implement `IAvailabilityService` per PRD 6.4 algorithm
- [ ] 5.5 Define and implement `IAppointmentService` (book, list, cancel by owner, cancel by token, complete)

## 6. Dependency injection and verification

- [ ] 6.1 Register all services, policy, and `ITimeProvider` in `AddApplication()`
- [ ] 6.2 Run `dotnet build` on the solution and fix any compile errors
