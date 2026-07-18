# Domain Model

The domain layer (`src/HoraMarcada.Domain/`) holds entities, value objects, enums, and domain exceptions. EF mappings live in Infrastructure but entities contain business invariants and state transitions.

## Entity relationship overview

```
Supabase Auth User (external)
  └── Store (1 per entrepreneur in practice)
        ├── Professional (1:N)
        │     └── WeeklyHours (1:N, per day of week)
        ├── Service (1:N)
        │     └── ServiceProfessional (N:N join table, Infrastructure)
        ├── Customer (1:N)
        └── Appointment (1:N) → Professional, Service, Customer
              TimeBlock (per professional, blocks availability)
```

## Entities

### Store

Aggregate root and tenant boundary. Source: `Entities/Store.cs`.

- Linked to Supabase user via `UserId`
- `Slug` — unique public URL identifier
- `WhatsApp`, `Email`, `Address`, optional geo coordinates
- `Active` flag

### Professional

A person who performs services. Source: `Entities/Professional.cs`.

- Belongs to one `StoreId`
- Optional `WhatsApp` for notifications
- `WeeklySchedules` collection of `WeeklyHours`
- `Active` flag (deactivation, not deletion)

### WeeklyHours

Working hours for one day. Source: `Entities/WeeklyHours.cs`.

- `DayOfWeek` 0–6 (Sunday–Saturday)
- `StartTime`, `EndTime`, optional lunch window
- `IntervalMinutes` — slot grid step size

### Service

Bookable offering. Source: `Entities/Service.cs`.

- `DurationMinutes` > 0
- Optional `Price` (`Money` value object)
- N:N with professionals via `ProfessionalIds`

### Customer

Per-store customer record. Source: `Entities/Customer.cs`.

- `Name`, `WhatsApp`
- Deduplicated by WhatsApp per store at booking time

### Appointment

Scheduled time slot. Source: `Entities/Appointment.cs`.

- References store, professional, service, customer
- Denormalized `CustomerName` / `CustomerWhatsApp` on the entity (also available via `Customer` navigation)
- `CancelToken` — opaque token for customer cancel/confirm links
- `CancelTokenUsed` — set when customer cancels via token

**Status enum** (`Enums/AppointmentStatus.cs`):

| Status | Meaning |
|--------|---------|
| `Confirmed` | Default after booking |
| `ConfirmedByUser` | Customer confirmed presence via token link |
| `Canceled` | Canceled by entrepreneur or customer |
| `Completed` | Marked done by entrepreneur |

State transitions are methods on the entity (`Cancel()`, `Complete()`, `ConfirmPresence()`, `MarkCancelTokenUsed()`). Invalid transitions throw `DomainValidationException`.

### TimeBlock

Administrative unavailability for a professional. Source: `Entities/TimeBlock.cs`.

- `StartTime`, `EndTime` range
- Blocks slot generation but does not cancel existing appointments

## Value objects

| Type | File | Rules |
|------|------|-------|
| `Slug` | `ValueObjects/Slug.cs` | Lowercase alphanumeric and hyphens |
| `PhoneNumber` | `ValueObjects/PhoneNumber.cs` | E.164-friendly format |
| `Email` | `ValueObjects/Email.cs` | Valid email format |
| `Money` | `ValueObjects/Money.cs` | Decimal price amounts |

## Key invariants

- Appointment `EndTime` must be after `StartTime`
- Cannot cancel a completed appointment or complete a canceled one
- WeeklyHours lunch interval must be fully within working hours or absent entirely
- Service duration must fit within a professional's working window for a slot to be offered

## Persistence notes

- `ServiceProfessional` join entity is in Infrastructure (`Persistence/ServiceProfessional.cs`), not Domain
- Recent migrations removed `email` columns from professionals and customers; contact is WhatsApp-only for notifications
- `Store.Email` remains required on create/update but is not used by the notification system

## Where to look when changing

| Concern | Files |
|---------|-------|
| Entity rules | `src/HoraMarcada.Domain/Entities/` |
| EF column mapping | `src/HoraMarcada.Infrastructure/Persistence/Configurations/` |
| OpenSpec domain spec | `openspec/specs/domain-model/spec.md` |
