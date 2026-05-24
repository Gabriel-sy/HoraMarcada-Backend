# Domain Model and Business Logic

This document details the business domain model, invariants, scheduling calculations, and core data flows of the Marque application.

---

## 1. Domain Entities & Value Objects

The domain layer encapsulates entities and value objects containing business rules and invariants.

### Store (Aggregate Root)
- **Role**: The tenant boundary. All professionals, services, and appointments belong to a specific store.
- **Invariants**:
  - `UserId` must not be empty.
  - `Name` is required (non-blank).
  - `Slug` must be a valid, unique URL-safe string.
  - `WhatsApp` must be a valid format.

### Professional
- **Role**: A resource performing services within a store.
- **Invariants**:
  - Associated with a valid `StoreId`.
  - Must have a `Name`.
  - Availability is determined by linked `WeeklyHours`.

### WeeklyHours
- **Role**: Configures the daily working hours and intervals for a professional.
- **Invariants**:
  - `DayOfWeek` must be a value between 0 (Sunday) and 6 (Saturday).
  - `StartTime` must be less than `EndTime`.
  - `LunchStart` and `LunchEnd` must either both be present or both be null.
  - If lunch is defined, the lunch interval must fall entirely within the `StartTime` and `EndTime` boundary.

### Service
- **Role**: A service offered by the store that takes a specific amount of time.
- **Invariants**:
  - `DurationMinutes` must be greater than zero.
  - Associated with professionals using an N:N relationship.

### Appointment (Aggregate)
- **Role**: Represents a scheduled slot.
- **Invariants**:
  - `EndTime` must be after `StartTime`.
  - State transitions are strictly controlled via domain methods. You cannot cancel an appointment that has already been completed, and you cannot complete an appointment that has been canceled.

### Value Objects
- **Slug**: Enforces lowercase alphanumeric characters and hyphens only (`^[a-z0-9]+(?:-[a-z0-9]+)*$`).
- **PhoneNumber**: Validates E.164-friendly phone formats.
- **Money**: Represents price amounts using decimal numbers.

---

## 2. Slot Availability Calculation Flow

Calculating free slots is a core requirement and is implemented in the application layer.

```
                  Input: storeId, serviceId, date, [professionalId]
                                         │
                         Validate store & active service
                                         │
                      Determine candidate professionals
                                         │
            For each professional, fetch WeeklyHours & Appointments
                                         │
                    Loop from StartTime to EndTime step Interval
                                         │
                     Check: slot + duration <= EndTime?
                                         │
                     Check: overlaps lunch interval?
                                         │
                 Check: overlaps existing appointments?
                                         │
                           Collect non-overlapping slots
                                         │
                       Order and return unique slots
```

### Calculation Steps
1. **Validation**: Check that the store and service exist, that the service belongs to the store, and that the service is active.
2. **Professional Selection**: If a specific `professionalId` is requested, verify they are active and assigned to this service. If no professional is specified, get all active professionals assigned to this service.
3. **Weekly Hours Lookup**: For each candidate professional, fetch the `WeeklyHours` configuration corresponding to the day-of-week of the target date.
4. **Existing Appointments Fetch**: Load all appointments for the professional on the target date whose status is not `Canceled`.
5. **Slot Generation**:
   - Begin at `StartTime`.
   - Generate slots incremented by the professional's `IntervalMinutes`.
   - Stop when the slot start time plus the service duration exceeds the professional's `EndTime`.
6. **Lunch Overlap Filtering**: Exclude slots where `slotStart < LunchEnd` and `slotEnd > LunchStart`.
7. **Appointment Overlap Filtering**: Exclude slots where `slotStart < appointment.EndTime` and `slotEnd > appointment.StartTime`.
8. **Aggregation**: Collect all valid slots, sort them chronologically, and return unique slot starting times.

---

## 3. Booking and Cancellation Flows

### The Scheduling Flow
1. Customer accesses the public booking page.
2. Customer selects a service, professional (or "any"), date, and slot time.
3. Customer inputs their name and WhatsApp number.
4. API endpoint: `POST /api/public/{slug}/appointments`.
5. **Double-Booking Prevention**: The use case queries available slots. If the selected slot is no longer available (e.g., booked in a race condition), it throws a domain exception.
6. The system generates a single-use signed cancellation JWT token.
7. The appointment is persisted to the database.
8. WhatsApp notifications are dispatched asynchronously to the customer, the professional, and the store owner. (Not implemented yet)

### Token-Based Cancellation Flow
1. The customer clicks the cancellation link sent to their WhatsApp.
2. API endpoint: `GET /api/public/cancel/{token}`.
3. The use case validates the JWT token:
   - Verifies the cryptographic signature.
   - Verifies the expiration claim.
   - Verifies the token has not been marked used in the database.
4. The appointment status is changed to `Canceled`.
5. The token is marked used (`CancelTokenUsed = true`).
6. WhatsApp alerts are sent to the store owner and the professional. (Not implemented yet)

---

## 4. Soft Deactivation Semantics

To preserve historical metrics, financial records, and schedules, deleting entities is restricted:
- **Professionals**: Deactivated via `Deactivate()`. They remain in the database but are ignored during slot calculations and cannot receive new appointments.
- **Services**: Deactivated via `Deactivate()`. Existing appointments remain unaffected, but new appointments cannot be created.
