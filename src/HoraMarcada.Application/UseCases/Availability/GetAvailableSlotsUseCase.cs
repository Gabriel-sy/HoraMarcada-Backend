using HoraMarcada.Application.Common.Interfaces;

namespace HoraMarcada.Application.UseCases.Availability;

public class GetAvailableSlotsUseCase(IAvailabilityService availabilityService)
{
    public Task<IReadOnlyList<DateTimeOffset>> ExecuteAsync(
        Guid storeId,
        Guid serviceId,
        DateTimeOffset date,
        Guid? professionalId = null,
        CancellationToken cancellationToken = default)
    {
        return availabilityService.GetAvailableSlotsAsync(storeId, serviceId, date, professionalId, cancellationToken);
    }
}
