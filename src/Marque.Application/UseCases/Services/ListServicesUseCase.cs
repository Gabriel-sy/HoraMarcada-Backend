using Marque.Application.Repositories;
using Marque.Domain.Entities;

namespace Marque.Application.UseCases.Services;

public class ListServicesUseCase(IServiceRepository serviceRepository)
{
    public async Task<IReadOnlyList<Service>> ExecuteAsync(Guid storeId, bool activeOnly, CancellationToken cancellationToken = default)
    {
        var services = await serviceRepository.ListByStoreIdAsync(storeId, cancellationToken);
        if (activeOnly)
        {
            return services.Where(s => s.Active).ToList();
        }
        return services;
    }
}
