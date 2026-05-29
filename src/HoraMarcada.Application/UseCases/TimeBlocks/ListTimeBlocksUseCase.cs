using HoraMarcada.Application.Exceptions;
using HoraMarcada.Application.Policies;
using HoraMarcada.Application.Repositories;
using HoraMarcada.Domain.Entities;

namespace HoraMarcada.Application.UseCases.TimeBlocks;

public class ListTimeBlocksUseCase(
    ITimeBlockRepository timeBlockRepository,
    IProfessionalRepository professionalRepository,
    IStoreOwnershipPolicy ownershipPolicy)
{
    public async Task<IReadOnlyList<TimeBlock>> ExecuteAsync(
        Guid userId,
        Guid storeId,
        Guid professionalId,
        DateTimeOffset start,
        DateTimeOffset end,
        CancellationToken cancellationToken = default)
    {
        await ownershipPolicy.EnsureUserOwnsStoreAsync(userId, storeId, cancellationToken);

        var professional = await professionalRepository.GetByIdAsync(professionalId, cancellationToken);
        if (professional == null || professional.StoreId != storeId)
        {
            throw new NotFoundException($"Professional with ID {professionalId} not found in store {storeId}.");
        }

        return await timeBlockRepository.ListByProfessionalAndRangeAsync(professionalId, start, end, cancellationToken);
    }
}
