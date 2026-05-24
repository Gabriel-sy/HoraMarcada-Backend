using Marque.Application.Common;

namespace Marque.Application.Models;

public record BookAppointmentInput(
    [property: SwaggerExample("3fa85f64-5717-4562-b3fc-2c963f66afa6")] Guid StoreId,
    [property: SwaggerExample("40049e6d-5bf4-4903-8d07-ee6456f9ee5b")] Guid ProfessionalId,
    [property: SwaggerExample("10022a1d-1bf4-4903-8d07-ee6456f9ee5c")] Guid ServiceId,
    [property: SwaggerExample("John Doe")] string CustomerName,
    [property: SwaggerExample("+5511988888888")] string CustomerWhatsApp,
    [property: SwaggerExample("2026-06-01T10:00:00Z")] DateTimeOffset StartTime);

public record ListAppointmentsInput(
    [property: SwaggerExample("3fa85f64-5717-4562-b3fc-2c963f66afa6")] Guid StoreId,
    [property: SwaggerExample("2026-06-01T00:00:00Z")] DateTimeOffset Start,
    [property: SwaggerExample("2026-06-07T23:59:59Z")] DateTimeOffset End,
    [property: SwaggerExample("40049e6d-5bf4-4903-8d07-ee6456f9ee5b")] Guid? ProfessionalId = null);

public record CancelAppointmentInput(
    [property: SwaggerExample("3fa85f64-5717-4562-b3fc-2c963f66afa6")] Guid StoreId);

public record CancelAppointmentByTokenInput(
    [property: SwaggerExample("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...")] string CancelToken);

public record CompleteAppointmentInput(
    [property: SwaggerExample("3fa85f64-5717-4562-b3fc-2c963f66afa6")] Guid StoreId);
