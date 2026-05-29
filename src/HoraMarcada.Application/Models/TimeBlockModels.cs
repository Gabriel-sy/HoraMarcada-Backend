using HoraMarcada.Application.Common;

namespace HoraMarcada.Application.Models;

public record CreateTimeBlockInput(
    [property: SwaggerExample("2026-06-01T14:00:00Z")] DateTimeOffset StartTime,
    [property: SwaggerExample("2026-06-01T15:30:00Z")] DateTimeOffset EndTime);
