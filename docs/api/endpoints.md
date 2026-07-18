# API Endpoints

Base URL: `http://localhost:5213` (development). All routes are prefixed with `api/`.

Swagger: `/swagger` (Development only).

## Authentication

Entrepreneur endpoints require `Authorization: Bearer <jwt>` where the JWT is issued by Supabase Auth. Obtain tokens via Supabase client SDK or auth API — see root `curl.md` for a password-grant example against Supabase (not the HoraMarcada API).

Controllers without `[Authorize]` are public.

## Stores

| Method | Route | Auth | Controller |
|--------|-------|------|------------|
| POST | `/api/stores` | Yes | `Stores/Create/CreateStoreController.cs` |
| GET | `/api/stores/user/{userId}` | Yes | `Stores/GetByUserId/GetStoreByUserIdController.cs` |
| GET | `/api/stores/slug/{slug}` | Public | `Stores/GetBySlug/GetStoreBySlugController.cs` |
| PUT | `/api/stores/{storeId}` | Yes | `Stores/Update/UpdateStoreController.cs` |
| GET | `/api/stores/{storeId}/statistics` | Yes | `Stores/GetStatistics/GetStoreStatisticsController.cs` |

## Professionals

| Method | Route | Auth | Controller |
|--------|-------|------|------------|
| POST | `/api/stores/{storeId}/professionals` | Yes | `Professionals/Add/AddProfessionalController.cs` |
| GET | `/api/stores/{storeId}/professionals` | Public | `Professionals/List/ListProfessionalsController.cs` |
| PUT | `/api/stores/{storeId}/professionals/{professionalId}` | Yes | `Professionals/Update/UpdateProfessionalController.cs` |
| POST | `/api/stores/{storeId}/professionals/{professionalId}/deactivate` | Yes | `Professionals/Deactivate/DeactivateProfessionalController.cs` |

## Services

| Method | Route | Auth | Controller |
|--------|-------|------|------------|
| POST | `/api/stores/{storeId}/services` | Yes | `Services/Add/AddServiceController.cs` |
| GET | `/api/stores/{storeId}/services` | Public | `Services/List/ListServicesController.cs` |
| PUT | `/api/stores/{storeId}/services/{serviceId}` | Yes | `Services/Update/UpdateServiceController.cs` |
| DELETE | `/api/stores/{storeId}/services/{serviceId}` | Yes | `Services/Delete/DeleteServiceController.cs` |
| GET | `/api/stores/{storeId}/services/{serviceId}/professionals` | Public | `Services/ListProfessionals/ListProfessionalsForServiceController.cs` |

## Availability

| Method | Route | Auth | Controller |
|--------|-------|------|------------|
| GET | `/api/stores/{storeId}/availability` | Public | `Availability/GetSlots/GetAvailableSlotsController.cs` |

Query parameters: `serviceId`, `date`, optional `professionalId`.

## Appointments

| Method | Route | Auth | Controller |
|--------|-------|------|------------|
| POST | `/api/appointments` | Public | `Appointments/Book/BookAppointmentController.cs` |
| GET | `/api/appointments` | Yes | `Appointments/List/ListAppointmentsController.cs` |
| GET | `/api/appointments/{appointmentId}` | Yes | `Appointments/Get/GetAppointmentController.cs` |
| POST | `/api/appointments/{appointmentId}/cancel` | Yes | `Appointments/Cancel/CancelAppointmentController.cs` |
| POST | `/api/appointments/{appointmentId}/complete` | Yes | `Appointments/Complete/CompleteAppointmentController.cs` |
| POST | `/api/appointments/cancel-by-token` | Public | `Appointments/CancelByToken/CancelAppointmentByTokenController.cs` |
| POST | `/api/appointments/confirm-by-token` | Public | `Appointments/ConfirmByToken/ConfirmAppointmentByTokenController.cs` |

## Customers

| Method | Route | Auth | Controller |
|--------|-------|------|------------|
| GET | `/api/stores/{storeId}/customers` | Yes | `Customers/List/ListCustomersController.cs` |
| GET | `/api/stores/{storeId}/customers/{customerId}` | Yes | `Customers/Get/GetCustomerController.cs` |
| PUT | `/api/stores/{storeId}/customers/{customerId}` | Yes | `Customers/Update/UpdateCustomerController.cs` |

## Time blocks

| Method | Route | Auth | Controller |
|--------|-------|------|------------|
| POST | `/api/stores/{storeId}/professionals/{professionalId}/blocks` | Yes | `TimeBlocks/Create/CreateTimeBlockController.cs` |
| GET | `/api/stores/{storeId}/professionals/{professionalId}/blocks` | Yes | `TimeBlocks/List/ListTimeBlocksController.cs` |
| DELETE | `/api/stores/{storeId}/professionals/{professionalId}/blocks/{blockId}` | Yes | `TimeBlocks/Delete/DeleteTimeBlockController.cs` |

## Error responses

Errors return RFC 7807 ProblemDetails via `GlobalExceptionHandler`. Validation failures from FluentValidation return 400 with field-level detail.

## Adding a new endpoint

1. Create use case in `Application/UseCases/`.
2. Register in `ApplicationServiceCollectionExtensions`.
3. Add controller + validator under `Api/Controllers/{Resource}/{Action}/`.
4. Add unit tests under `tests/HoraMarcada.UnitTests/UseCases/`.
5. Update this page.

OpenSpec change `openspec/changes/implement-controllers/` documents the controller-per-action convention.
