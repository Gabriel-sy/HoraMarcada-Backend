using HoraMarcada.Application.Common;
using HoraMarcada.Domain.Entities;

namespace HoraMarcada.Application.Models;

public record TimeBlockDto(
    Guid Id,
    Guid StoreId,
    Guid ProfessionalId,
    DateTimeOffset StartTime,
    DateTimeOffset EndTime,
    DateTimeOffset CreatedAt)
{
    public static TimeBlockDto FromDomain(TimeBlock timeBlock) => new(
        timeBlock.Id,
        timeBlock.StoreId,
        timeBlock.ProfessionalId,
        timeBlock.StartTime,
        timeBlock.EndTime,
        timeBlock.CreatedAt);
}

public record CreateTimeBlockInput(
    [property: SwaggerExample("2026-06-01T14:00:00Z")] DateTimeOffset StartTime,
    [property: SwaggerExample("2026-06-01T15:30:00Z")] DateTimeOffset EndTime);
