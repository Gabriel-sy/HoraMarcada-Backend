namespace Marque.Application.Models;

public record AddServiceInput(
    Guid UserId,
    Guid StoreId,
    string Name,
    short DurationMinutes,
    decimal? Price = null,
    List<Guid>? ProfessionalIds = null);

public record UpdateServiceInput(
    Guid UserId,
    Guid StoreId,
    Guid ServiceId,
    string Name,
    short DurationMinutes,
    decimal? Price = null,
    bool Active = true,
    List<Guid>? ProfessionalIds = null);
