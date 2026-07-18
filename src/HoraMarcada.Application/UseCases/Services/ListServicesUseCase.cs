using HoraMarcada.Application.Models;
using HoraMarcada.Application.Repositories;

namespace HoraMarcada.Application.UseCases.Services;

public class ListServicesUseCase(IServiceRepository serviceRepository)
{
    public async Task<IReadOnlyList<ServiceDto>> ExecuteAsync(Guid storeId, bool activeOnly, CancellationToken cancellationToken = default)
    {
        var services = await serviceRepository.ListByStoreIdAsync(storeId, cancellationToken);
        if (activeOnly)
        {
            return services.Where(s => s.Active).Select(ServiceDto.FromDomain).ToList();
        }
        return services.Select(ServiceDto.FromDomain).ToList();
    }
}
