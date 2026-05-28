using HoraMarcada.Application.Exceptions;
using HoraMarcada.Application.Policies;
using HoraMarcada.Application.Repositories;

namespace HoraMarcada.Application.UseCases.Services;

public class DeleteServiceUseCase(
    IServiceRepository serviceRepository,
    IStoreOwnershipPolicy ownershipPolicy)
{
    public async Task ExecuteAsync(Guid userId, Guid storeId, Guid serviceId, CancellationToken cancellationToken = default)
    {
        await ownershipPolicy.EnsureUserOwnsStoreAsync(userId, storeId, cancellationToken);

        var service = await serviceRepository.GetByIdAsync(serviceId, cancellationToken);
        if (service == null || service.StoreId != storeId)
        {
            throw new NotFoundException($"Service with ID {serviceId} not found in store {storeId}.");
        }

        await serviceRepository.DeleteAsync(service, cancellationToken);
    }
}
