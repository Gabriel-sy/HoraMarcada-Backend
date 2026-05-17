## Why

The backend has domain entities, repository ports, and EF persistence, but no application services orchestrating business workflows. The PRD requires store CRUD, professional/service management, availability calculation, and appointment lifecycle (book, cancel, complete) with ownership checks before any API or WhatsApp work. Without use-case interfaces and policies in the Application layer, controllers would embed business rules and violate DIP.

## What Changes

- Define application service interfaces (use-case ports) in `Marque.Application` for stores, professionals, services, availability, and appointments
- Implement use cases as application services that depend only on repository interfaces and policy abstractions — no EF, HTTP, JWT, or WhatsApp types
- Add ownership/authorization policy interface (`IStoreOwnershipPolicy`) and implementation using repositories to verify the authenticated user owns a store
- Add availability calculation as an application service implementing PRD section 6.4 (weekly hours, lunch, interval, appointment collision)
- Extend repository ports where use cases need queries not yet exposed (e.g. appointments by professional/date range, store by user id)
- Enrich domain entities with state transitions where rules belong in Domain (`Appointment.Cancel`, `Appointment.Complete`, etc.)
- Register all application services and policies in `AddApplication()`
- Unit-testable use cases with no Infrastructure references from Application implementations

## Capabilities

### New Capabilities

- `use-case-ports`: Application service interfaces and DI registration contract for the API layer
- `ownership-policy`: Store ownership verification policy (user owns store) as an injectable abstraction
- `store-use-cases`: Create, get, and update store for authenticated entrepreneur
- `professional-use-cases`: Add, update, list, and deactivate professionals with weekly hours
- `service-use-cases`: Add, update, list services and assign professionals (N:N)
- `availability-calculation`: Free-slot calculation per PRD 6.4 for public booking flow
- `appointment-use-cases`: Book appointment (with conflict check), list for panel, cancel (entrepreneur and token), mark completed

### Modified Capabilities

- `domain-model`: Appointment and related entity behavior for cancel/complete/deactivate transitions enforced in Domain

## Impact

- **Code**: `Marque.Application` (Services/, Policies/, DTOs or commands as needed); `Marque.Domain` (entity methods); repository interface extensions in Application
- **Dependencies**: Application remains free of EF Core and ASP.NET; may add minimal abstractions for time (`IClock`) if needed for testability
- **Systems**: No API endpoints, Supabase JWT middleware, WhatsApp, Hangfire, or cancel-token JWT signing in this change — those consume these ports next
- **Out of scope**: HTTP controllers, MediatR, integration tests, notification dispatch, cancel-token generation
