# WhatsApp Integration

All customer and professional notifications go through WhatsApp via [WAHA](https://github.com/devlikeapro/waha) (WhatsApp HTTP API). Email notifications were removed; the `openspec/specs/email-notifications/spec.md` spec is stale.

## Architecture

```
Use case (e.g. BookAppointment)
  → IAppointmentNotificationService
    → INotificationService (QueueNotificationServiceDecorator)
      → INotificationDispatcher (WahaWhatsAppNotificationService)
        → IWahaClient (HTTP to WAHA)
```

Kill switch: `NOTIFICATIONS_ENABLED` (default on) via `IAppConfiguration`.

## Notification events

Defined on `INotificationService` (`Application/Common/Interfaces/INotificationService.cs`):

| Method | Trigger |
|--------|---------|
| `SendBookingConfirmationToCustomerAsync` | After booking |
| `SendBookingNotificationToProfessionalAsync` | After booking (if professional has WhatsApp) |
| `SendCancellationNoticeToCustomerAsync` | After cancel |
| `SendCancellationNotificationToProfessionalAsync` | After cancel (if professional has WhatsApp) |
| `SendBookingReminder24hAsync` | Hangfire `SendReminderJob` |
| `SendDailySummaryToProfessionalAsync` | Hangfire recurring job |

Orchestration: `AppointmentNotificationService` (`Application/Common/Services/AppointmentNotificationService.cs`).

## WAHA transport

`WahaWhatsAppNotificationService` (`Infrastructure/Notifications/Channels/WhatsApp/WahaWhatsAppNotificationService.cs`):

- Requires WAHA session status `WORKING`
- Validates number exists via `api/contacts/check-exists` before sending
- Sends text via `api/sendText`; may send location for store address
- Uses `FrontendUrl` for cancel/reschedule links (defaults to `https://horamarcada.site`)

HTTP client configured in Infrastructure DI with `Waha:BaseUrl` and `X-Api-Key` header from `Waha:ApiKey`.

Message templates: `Infrastructure/Notifications/Channels/WhatsApp/Formatting/WhatsAppMessageTemplates.cs`.

Human-behavior simulation (typing indicators, delays): `Waha:HumanBehavior` settings and `WhatsAppHumanBehavior.cs`.

## Async queue

Most sends are enqueued, not sent inline:

- `QueueNotificationServiceDecorator` — enqueues `NotificationJob`
- `NotificationQueueProcessor` — hosted service, 3 retries on failure

**Exception:** `SendDailySummaryToProfessionalAsync` calls the dispatcher directly (bypasses queue) to control pacing with 30–60s random delays between professionals.

## Background jobs

### 24h reminder — `SendReminderJob`

- Scheduled by `HangfireBackgroundJobService.ScheduleReminder` at booking time
- Loads appointment; sends only if `Status == Confirmed` (skips `ConfirmedByUser`, `Completed`, `Canceled`)
- Calls `SendBookingReminder24hAsync`

### Daily summary — `SendDailySummaryJob`

- Recurring cron: `0 8 * * *`, timezone `America/Sao_Paulo` (registered in `Program.cs`)
- Iterates active professionals with WhatsApp and active stores
- Includes appointments with status `Confirmed`, `ConfirmedByUser`, or `Completed` for the current day

## Docker local setup

`docker-compose.yml` includes a `waha` service (`devlikeapro/waha:latest`) on port 3000 with session volume `waha-sessions`. Backend connects via `Waha__BaseUrl: http://waha:3000`.

WAHA requires initial session pairing (QR code) outside this repo's scope.

## Configuration

| Key | Purpose |
|-----|---------|
| `Waha:BaseUrl` | WAHA server URL |
| `Waha:ApiKey` | API key for `X-Api-Key` header |
| `Waha:SessionName` | Named WAHA session |
| `Waha:HumanBehavior:*` | Delay/typing simulation |
| `NOTIFICATIONS_ENABLED` | Global on/off switch |

## Tests

- `tests/HoraMarcada.UnitTests/UseCases/Appointments/AppointmentNotificationServiceTests.cs` — scheduling logic, dispatch calls
- `tests/HoraMarcada.UnitTests/Infrastructure/Notifications/` — WAHA service, queue decorator, formatters
- `tests/HoraMarcada.UnitTests/Infrastructure/Jobs/SendDailySummaryJobTests.cs`

No tests for `SendReminderJob` execution or live WAHA integration.

## Change guidance

| Change | Files |
|--------|-------|
| New message type | `INotificationService`, `WahaWhatsAppNotificationService`, templates |
| New trigger point | Use case + `AppointmentNotificationService` |
| Template wording | `WhatsAppMessageTemplates.cs` |
| Link URLs | `AppointmentNotificationUrls.cs`, `FrontendUrl` config |

## Gaps vs PRD

- No entrepreneur WhatsApp on booking/cancel (only customer + professional)
- No 2h same-day reminder
- Daily summary targets professionals, not store owner
- `Store.Email` and `Store.WhatsApp` exist on the entity but store owner is not in the notification dispatch path for booking events
