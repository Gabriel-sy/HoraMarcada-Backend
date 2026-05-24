using HoraMarcada.Application.Common;

namespace HoraMarcada.Application.Models;

public record GetAvailableSlotsInput(
    [property: SwaggerExample("10022a1d-1bf4-4903-8d07-ee6456f9ee5c")] Guid ServiceId,
    [property: SwaggerExample("2026-06-01T00:00:00Z")] DateTimeOffset Date,
    [property: SwaggerExample("40049e6d-5bf4-4903-8d07-ee6456f9ee5b")] Guid? ProfessionalId = null);
    