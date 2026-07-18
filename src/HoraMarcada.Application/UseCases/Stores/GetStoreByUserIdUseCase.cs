using HoraMarcada.Application.Exceptions;
using HoraMarcada.Application.Models;
using HoraMarcada.Application.Repositories;

namespace HoraMarcada.Application.UseCases.Stores;

public class GetStoreByUserIdUseCase(IStoreRepository storeRepository)
{
    public async Task<StoreDto> ExecuteAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var store = await storeRepository.GetByUserIdAsync(userId, cancellationToken);
        if (store == null)
        {
            throw new NotFoundException($"Store for user {userId} not found.");
        }
        return StoreDto.FromDomain(store);
    }
}
