using HoraMarcada.Application.Exceptions;
using HoraMarcada.Application.Policies;
using HoraMarcada.Application.Repositories;

namespace HoraMarcada.Application.UseCases.TimeBlocks;

public class DeleteTimeBlockUseCase(
    ITimeBlockRepository timeBlockRepository,
    IStoreOwnershipPolicy ownershipPolicy)
{
    public async Task ExecuteAsync(
        Guid userId,
        Guid storeId,
        Guid blockId,
        CancellationToken cancellationToken = default)
    {
        await ownershipPolicy.EnsureUserOwnsStoreAsync(userId, storeId, cancellationToken);

        var timeBlock = await timeBlockRepository.GetByIdAsync(blockId, cancellationToken);
        if (timeBlock == null || timeBlock.StoreId != storeId)
        {
            throw new NotFoundException($"Time block with ID {blockId} not found in store {storeId}.");
        }

        await timeBlockRepository.DeleteAsync(timeBlock, cancellationToken);
    }
}
