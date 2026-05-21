using Marque.Application.Exceptions;
using Marque.Application.Repositories;

namespace Marque.Application.Policies;

public class StoreOwnershipPolicy(IStoreRepository storeRepository) : IStoreOwnershipPolicy
{
    public async Task EnsureUserOwnsStoreAsync(Guid userId, Guid storeId, CancellationToken cancellationToken = default)
    {
        var store = await storeRepository.GetByIdAsync(storeId, cancellationToken);
        if (store == null)
        {
            throw new NotFoundException($"Store with ID {storeId} not found.");
        }

        if (store.UserId != userId)
        {
            throw new NotFoundException($"Store with ID {storeId} not found.");
        }
    }
}
