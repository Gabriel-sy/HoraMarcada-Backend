using HoraMarcada.Application.Common;
using HoraMarcada.Domain.Entities;

namespace HoraMarcada.Application.Models;

public record ServiceDto(
    Guid Id,
    Guid StoreId,
    string Name,
    short DurationMinutes,
    decimal? Price,
    bool Active,
    IReadOnlyList<Guid> ProfessionalIds)
{
    public static ServiceDto FromDomain(Service service) => new(
        service.Id,
        service.StoreId,
        service.Name,
        service.DurationMinutes,
        service.Price?.Amount,
        service.Active,
        service.ProfessionalIds.ToList());
}

public record AddServiceInput(
    [property: SwaggerExample("Haircut")] string Name,
    [property: SwaggerExample("45")] short DurationMinutes,
    [property: SwaggerExample("50.00")] decimal? Price = null,
    List<Guid>? ProfessionalIds = null);

public record UpdateServiceInput(
    [property: SwaggerExample("Haircut Premium")] string Name,
    [property: SwaggerExample("45")] short DurationMinutes,
    [property: SwaggerExample("65.00")] decimal? Price = null,
    [property: SwaggerExample("true")] bool Active = true,
    List<Guid>? ProfessionalIds = null);
