using HoraMarcada.Application.Common;
using HoraMarcada.Domain.Entities;
using HoraMarcada.Domain.Enums;

namespace HoraMarcada.Application.Models;

public record AppointmentDto(
    Guid Id,
    Guid StoreId,
    Guid ProfessionalId,
    Guid ServiceId,
    Guid CustomerId,
    string CustomerName,
    string CustomerWhatsApp,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    AppointmentStatus Status,
    string? CancelToken,
    bool CancelTokenUsed,
    DateTimeOffset CreatedAt)
{
    public static AppointmentDto FromDomain(Appointment appointment) => new(
        appointment.Id,
        appointment.StoreId,
        appointment.ProfessionalId,
        appointment.ServiceId,
        appointment.CustomerId,
        appointment.CustomerName,
        appointment.CustomerWhatsApp.Value,
        appointment.StartTime,
        appointment.EndTime,
        appointment.Status,
        appointment.CancelToken,
        appointment.CancelTokenUsed,
        appointment.CreatedAt);
}

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

public record CancelAppointmentByTokenResponse(
    [property: SwaggerExample("vintage-barber")] string StoreSlug);

public record CompleteAppointmentInput(
    [property: SwaggerExample("3fa85f64-5717-4562-b3fc-2c963f66afa6")] Guid StoreId);

public record ConfirmAppointmentByTokenInput(
    [property: SwaggerExample("cancel_token_here")] string CancelToken);
