using HoraMarcada.Application.Models;
using HoraMarcada.Application.Repositories;

namespace HoraMarcada.Application.UseCases.Services;

public class ListServicesUseCase(
    IServiceRepository serviceRepository,
    ICategoryRepository categoryRepository)
{
    public async Task<IReadOnlyList<ServiceDto>> ExecuteAsync(Guid storeId, bool activeOnly, CancellationToken cancellationToken = default)
    {
        var services = await serviceRepository.ListByStoreIdAsync(storeId, cancellationToken);
        var categories = await categoryRepository.ListByStoreIdAsync(storeId, cancellationToken);
        var categoryNames = categories.ToDictionary(c => c.Id, c => c.Name);

        IEnumerable<Domain.Entities.Service> filtered = activeOnly
            ? services.Where(s => s.Active)
            : services;

        return filtered
            .Select(s => ServiceDto.FromDomain(s, categoryNames.GetValueOrDefault(s.CategoryId, string.Empty)))
            .ToList();
    }
}
