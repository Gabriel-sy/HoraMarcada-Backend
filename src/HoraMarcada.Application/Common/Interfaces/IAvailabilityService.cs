namespace HoraMarcada.Application.Common.Interfaces;

public interface IAvailabilityService
{
    Task<IReadOnlyList<DateTimeOffset>> GetAvailableSlotsAsync(
        Guid storeId,
        Guid serviceId,
        DateTimeOffset date,
        Guid? professionalId = null,
        CancellationToken cancellationToken = default);
}
