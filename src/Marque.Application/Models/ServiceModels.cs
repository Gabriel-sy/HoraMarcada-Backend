using Marque.Application.Common;

namespace Marque.Application.Models;

public record AddServiceInput(
    [property: SwaggerExample("3fa85f64-5717-4562-b3fc-2c963f66afa6")] Guid UserId,
    [property: SwaggerExample("Haircut")] string Name,
    [property: SwaggerExample("45")] short DurationMinutes,
    [property: SwaggerExample("50.00")] decimal? Price = null,
    List<Guid>? ProfessionalIds = null);

public record UpdateServiceInput(
    [property: SwaggerExample("3fa85f64-5717-4562-b3fc-2c963f66afa6")] Guid UserId,
    [property: SwaggerExample("Haircut Premium")] string Name,
    [property: SwaggerExample("45")] short DurationMinutes,
    [property: SwaggerExample("65.00")] decimal? Price = null,
    [property: SwaggerExample("true")] bool Active = true,
    List<Guid>? ProfessionalIds = null);
