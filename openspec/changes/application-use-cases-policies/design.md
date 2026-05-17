## Context

`dotnet-backend-foundation` delivered Domain entities, value objects, and repository ports. `supabase-ef-persistence` implemented EF repositories and PostgreSQL schema. `Marque.Application.AddApplication()` is empty; the API has no controllers. The PRD defines entrepreneur CRUD, public booking with availability, and appointment lifecycle with ownership enforced server-side (PRD 6.3, 6.4, 9.1).

The user requires strict DIP: **policies and use cases** live in Application/Domain; **details** (EF, JWT, HTTP, WhatsApp) stay in Infrastructure/API. API controllers will depend on application service interfaces only.

## Goals / Non-Goals

**Goals:**

- Application service interfaces for each PRD workflow area (store, professional, service, availability, appointment)
- Concrete use-case classes in `Marque.Application` implementing those interfaces via repository ports and policies
- `IStoreOwnershipPolicy` abstraction verifying `store.UserId == authenticatedUserId` before mutating store-scoped resources
- Availability algorithm matching PRD 6.4 (slots, lunch exclusion, appointment collision)
- Domain methods for appointment state transitions (`Cancel`, `Complete`) and entity deactivation where appropriate
- Extended repository query methods required by use cases (no business logic in repositories)
- `AddApplication()` registers interfaces → implementations as scoped services

**Non-Goals:**

- ASP.NET controllers, DTO mapping libraries, or minimal API endpoints
- Supabase JWT authentication middleware (callers pass `Guid userId` into private use cases for now)
- Cancel-token JWT generation/validation (`ICancelTokenService` — follow-up with Infrastructure)
- WhatsApp / `INotificationService` dispatch (use cases expose success; API/jobs trigger messaging later)
- Hangfire jobs, rate limiting, MediatR, FluentValidation pipeline
- Unit test project (recommended follow-up; use cases designed for isolation)
- Changing EF mappings or migrations

## Decisions

### 1. Folder layout in Application

**Decision:**

| Folder | Contents |
|--------|----------|
| `Services/` | Use-case interfaces and implementations |
| `Policies/` | `IStoreOwnershipPolicy` + `StoreOwnershipPolicy` |
| `Repositories/` | Existing ports; extend with new query methods |
| `Models/` | Input records for use cases (commands/queries), not HTTP DTOs |

**Rationale:** Clear separation of policies vs orchestration; API layer maps HTTP to `Models` records later.

### 2. Use-case interface naming

**Decision:** One interface per bounded workflow, suffixed with `Service` per PRD naming (`IStoreService`, `IProfessionalService`, `IServiceCatalogService`, `IAvailabilityService`, `IAppointmentService`).

**Rationale:** Aligns with PRD section 8.1 (`AvailabilityService`, `AppointmentService`); API injects interfaces.

**Alternative:** MediatR commands — rejected for MVP simplicity.

### 3. Ownership policy

**Decision:** `IStoreOwnershipPolicy` with `Task EnsureUserOwnsStoreAsync(Guid userId, Guid storeId, CancellationToken ct)` throwing `UnauthorizedAccessException` or a dedicated `ForbiddenException` in Application when ownership fails.

**Rationale:** Centralizes authorization rule; use cases call policy before mutations. JWT extraction stays in API; use cases receive `userId` as parameter.

### 4. Availability as pure application logic

**Decision:** `AvailabilityService` implements PRD 6.4 using:

- `IProfessionalRepository` to load professional + `WeeklyHours` for `date.DayOfWeek`
- `IAppointmentRepository.ListByProfessionalAndDateAsync` for non-canceled appointments
- `IServiceRepository` for service duration
- In-memory slot generation (no DB-side slot function)

**Rationale:** Algorithm is business-critical and testable without HTTP; matches PRD pseudocode.

**"Any available" professional:** `IAvailabilityService` accepts optional `professionalId`; when null, union slots across professionals linked to the service (first free slot wins per PRD step 2).

### 5. Appointment booking and conflicts

**Decision:** `BookAppointmentAsync` re-runs availability check for the chosen slot before insert (optimistic concurrency per PRD 9.1). Domain creates `Appointment` via factory; repository persists.

**Rationale:** Prevents double booking at application layer even before DB unique constraints.

### 6. Repository extensions

**Decision:** Add to Application ports (Infrastructure implements):

| Interface | New methods |
|-----------|-------------|
| `IStoreRepository` | `GetByUserIdAsync`, `UpdateAsync` |
| `IAppointmentRepository` | `ListByStoreAsync(storeId, start, end, professionalId?)`, `ListByProfessionalAndDateAsync`, `GetByCancelTokenAsync`, `UpdateAsync` |
| `IProfessionalRepository` | `UpdateAsync` |
| `IServiceRepository` | `UpdateAsync`, `ListByServiceIdProfessionalsAsync` (or load via store aggregate) |

Repositories remain persistence-only (no validation).

### 7. Domain entity enrichments

**Decision:**

- `Appointment.Cancel()` — sets status `Canceled`, validates not already canceled/completed
- `Appointment.Complete()` — sets status `Completed`
- `Appointment.MarkCancelTokenUsed()` — for token cancellation flow
- `Professional.Deactivate()`, `Service.Deactivate()` — soft delete per PRD
- Optional update methods with validation on `Store`, `Professional`, `Service` where PRD allows edits

**Rationale:** State transition rules stay in Domain; use cases orchestrate.

### 8. Time abstraction

**Decision:** Introduce `ITimeProvider` (`DateTimeOffset UtcNow`) in Application, default `SystemTimeProvider` registered in DI.

**Rationale:** Enables unit tests for availability and appointment windows without static `DateTimeOffset.UtcNow`.

### 9. Error model

**Decision:** Use existing `DomainValidationException` for invariant violations; add `NotFoundException` and `ConflictException` in Application for use-case-level failures (store not found, slot no longer available).

**Rationale:** API layer can map to 404/409/400 later without try/catch on EF types.

### 10. DIP registration

**Decision:** `AddApplication()` registers all `I*Service` → implementations and `IStoreOwnershipPolicy`, `ITimeProvider` as scoped.

**Rationale:** Single composition root; Infrastructure does not register application services.

## Risks / Trade-offs

| Risk | Mitigation |
|------|------------|
| Race condition on booking | Re-check availability immediately before insert; document DB transaction/unique index as future hardening |
| Repository `UpdateAsync` vs aggregate tracking | EF implementations attach/update entities explicitly; keep update methods idempotent |
| "Any professional" performance | Load professionals for service only; cap date range to single day per PRD public flow |
| Use cases accept raw `Guid userId` | Document that API must validate JWT before calling; add auth middleware in next change |
| Partial store aggregate loads | Ownership policy loads store by id only; use cases load full graph when needed |

## Migration Plan

1. Extend domain entities and repository interfaces
2. Implement Application services and policy
3. Extend EF repositories for new port methods
4. Wire `AddApplication()` and verify `dotnet build`
5. Next change: API controllers + Supabase JWT middleware consuming interfaces

Rollback: remove service registrations; no database migration required.

## Open Questions

- Whether `ForbiddenException` should live in Application or Domain — prefer Application
- Cancel-token signing interface shape deferred to auth/infrastructure change
- Whether `IServiceCatalogService` naming avoids clash with domain `Service` entity — use `IServiceOfferingService` if compiler ambiguity arises
