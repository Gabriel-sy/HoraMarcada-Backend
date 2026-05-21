## ADDED Requirements

### Requirement: Calculate free slots per PRD algorithm

`IAvailabilityService` SHALL compute available time slots for a given store, service, date, and optional professional id following PRD section 6.4:

1. Load weekly hours for the professional on the date's day of week
2. Generate slots from start to end using `interval_min`
3. Exclude slots overlapping lunch when lunch times are set
4. Load appointments for the professional on that date with status not canceled
5. Exclude slots where `[slot, slot + service_duration]` overlaps any appointment
6. Return remaining slots as `TimeOnly` or `DateTimeOffset` values in store-local semantics documented in design

#### Scenario: Lunch interval excludes slots

- **WHEN** weekly hours include lunch start and end
- **THEN** no returned slot overlaps the lunch interval

#### Scenario: Booked appointment excludes overlapping slots

- **WHEN** a confirmed appointment exists on the date
- **THEN** slots whose interval overlaps the appointment are not returned

#### Scenario: Canceled appointments do not block slots

- **WHEN** an appointment on the date has status canceled
- **THEN** its time range does not reduce availability

### Requirement: No weekly hours yields empty availability

When the professional has no weekly hours for the requested day of week, the service SHALL return an empty slot list.

#### Scenario: Closed day

- **WHEN** availability is requested for a day without weekly hours
- **THEN** the result is an empty list

### Requirement: Any-available professional mode

When `professionalId` is not specified, the service SHALL aggregate availability across all active professionals assigned to the service and return the union of free slots (or earliest slot per PRD "any available" behavior defined in design).

#### Scenario: Union across professionals

- **WHEN** availability is requested without a professional id for a service with two professionals
- **THEN** returned slots include any slot free for at least one assigned professional

### Requirement: Service duration applied to slot length

Each candidate slot SHALL be validated using the service's `DurationMinutes`, not only the grid interval.

#### Scenario: Slot too short for service

- **WHEN** a grid slot remains but service duration would extend past working hours or into lunch
- **THEN** that slot is not returned
