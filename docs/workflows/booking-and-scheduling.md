# Booking & Scheduling

Core scheduling workflows: availability calculation, public booking, entrepreneur management, and token-based customer actions.

## Public booking flow

1. Customer opens store public page (`GET /api/stores/slug/{slug}`).
2. Lists services and professionals (public endpoints).
3. Queries available slots (`GET /api/stores/{storeId}/availability`).
4. Submits booking (`POST /api/appointments`) with name, WhatsApp, service, professional, and slot time.
5. Receives WhatsApp confirmation with cancel/reschedule links.

Implementation: `BookAppointmentUseCase` (`Application/UseCases/Appointments/BookAppointmentUseCase.cs`).

### Booking validation

1. Reject slots in the past.
2. Verify store, service, and professional are active.
3. Verify professional is assigned to the service.
4. Re-check slot availability via `IAvailabilityService` (race-condition guard).
5. Upsert `Customer` by WhatsApp within the store.
6. Create `Appointment` with unique `CancelToken`.
7. Dispatch notifications via `IAppointmentNotificationService`.

## Availability calculation

Implemented in `AvailabilityService` (`Application/Common/Services/AvailabilityService.cs`), called by `GetAvailableSlotsUseCase`.

### Inputs

- `storeId`, `serviceId`, `date` (with offset), optional `professionalId`

### Algorithm

```
Validate store and active service
  → Select candidate professionals (one or all assigned to service)
  → For each professional:
       Load WeeklyHours for day-of-week
       Load non-canceled appointments and time blocks for that date
       Generate slots from StartTime to EndTime stepping by IntervalMinutes
       Exclude: lunch overlap, time blocks, existing appointments, past slots
  → Union, deduplicate, sort, return
```

### Lead time

Slots too close to "now" are excluded using `IAppConfiguration.MinimumSlotLeadTimeMinutes` (`MINIMUM_SLOT_LEAD_TIME_MINUTES` env/config).

### Without professional filter

Returns the union of available slots across all active professionals assigned to the service. The customer picks a slot; booking still requires a specific `professionalId`.

## Time blocks

Entrepreneurs block periods when a professional is unavailable (vacation, breaks not covered by lunch config).

- Create: `POST /api/stores/{storeId}/professionals/{professionalId}/blocks`
- List / delete: corresponding endpoints under `TimeBlocks/`

Time blocks affect availability but do not modify existing appointments.

## Entrepreneur appointment management

| Action | Endpoint | Use case |
|--------|----------|----------|
| List appointments | `GET /api/appointments` | `ListAppointmentsUseCase` (filtered by owned store) |
| Get by ID | `GET /api/appointments/{id}` | `GetAppointmentByIdUseCase` |
| Cancel | `POST /api/appointments/{id}/cancel` | `CancelAppointmentUseCase` |
| Complete | `POST /api/appointments/{id}/complete` | `CompleteAppointmentUseCase` |

All require JWT auth and store ownership checks.

## Token-based customer actions

Customers interact via links in WhatsApp messages. Both cancel and confirm use the same `CancelToken` field on the appointment.

### Cancel by token

`POST /api/appointments/cancel-by-token` — `CancelAppointmentByTokenUseCase`

1. Find appointment by token.
2. Reject if token already used or appointment not cancelable.
3. `appointment.Cancel()` and `MarkCancelTokenUsed()`.
4. Send cancellation WhatsApp to customer and professional (if WhatsApp set).

### Confirm by token

`POST /api/appointments/confirm-by-token` — `ConfirmAppointmentByTokenUseCase`

1. Find appointment by token.
2. `appointment.ConfirmPresence()` → status `ConfirmedByUser`.
3. Does **not** mark the token as used — customer can still cancel afterward.

## Notifications after booking/cancel

Orchestrated by `AppointmentNotificationService` (`Application/Common/Services/AppointmentNotificationService.cs`):

**On booking:**

- WhatsApp confirmation to customer
- WhatsApp notice to professional (if `professional.WhatsApp` is set)
- Schedule 24h reminder via Hangfire (`IBackgroundJobService.ScheduleReminder`)

**Reminder scheduling rules** (hardcoded UTC-3):

- Target time: 24h before appointment
- Quiet hours: clamp to 08:00–22:00 (late night → 21:00)
- Requires >6h lead time; otherwise fallback to 08:00 on appointment day

**On cancel:**

- WhatsApp to customer and optional professional

Details: [WhatsApp integration](../integrations/whatsapp.md).

## Store statistics

`GET /api/stores/{storeId}/statistics` — KPIs for a date range (default last 30 days). Implementation: `GetStoreStatisticsUseCase`.

## Tests to run

- `tests/HoraMarcada.UnitTests/Application/Common/AvailabilityServiceTests.cs`
- `tests/HoraMarcada.UnitTests/UseCases/Appointments/BookAppointmentUseCaseTests.cs`
- `tests/HoraMarcada.UnitTests/UseCases/Appointments/CancelAppointment*Tests.cs`
- `tests/HoraMarcada.UnitTests/UseCases/Appointments/AppointmentNotificationServiceTests.cs`

## Known product/code gaps

- `PRD.md` §4.6 lists entrepreneur notifications on booking/cancel and a 2h same-day reminder — not implemented; only customer/professional WhatsApp and 24h reminder exist.
- Daily summary goes to professionals at 08:00 (`America/Sao_Paulo`), not entrepreneurs at 07:00 as in PRD.
- Timezone handling uses appointment offset and UTC-3 for reminders; there is no per-store timezone setting.
