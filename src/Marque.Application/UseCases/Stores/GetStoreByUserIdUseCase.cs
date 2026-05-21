using Marque.Application.Exceptions;
using Marque.Application.Repositories;
using Marque.Domain.Entities;

namespace Marque.Application.UseCases.Stores;

public class GetStoreByUserIdUseCase(IStoreRepository storeRepository)
{
    public async Task<Store> ExecuteAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var store = await storeRepository.GetByUserIdAsync(userId, cancellationToken);
        if (store == null)
        {
            throw new NotFoundException($"Store for user {userId} not found.");
        }
        return store;
    }
}
