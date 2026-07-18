using HoraMarcada.Application.Exceptions;
using HoraMarcada.Application.Models;
using HoraMarcada.Application.Policies;
using HoraMarcada.Application.Repositories;
using HoraMarcada.Domain.Entities;

namespace HoraMarcada.Application.UseCases.TimeBlocks;

public class CreateTimeBlockUseCase(
    ITimeBlockRepository timeBlockRepository,
    IProfessionalRepository professionalRepository,
    IStoreOwnershipPolicy ownershipPolicy)
{
    public async Task<TimeBlockDto> ExecuteAsync(
        Guid userId,
        Guid storeId,
        Guid professionalId,
        CreateTimeBlockInput input,
        CancellationToken cancellationToken = default)
    {
        await ownershipPolicy.EnsureUserOwnsStoreAsync(userId, storeId, cancellationToken);

        var professional = await professionalRepository.GetByIdAsync(professionalId, cancellationToken);
        if (professional == null || professional.StoreId != storeId)
        {
            throw new NotFoundException($"Professional with ID {professionalId} not found in store {storeId}.");
        }

        var timeBlock = TimeBlock.Create(
            storeId,
            professionalId,
            input.StartTime,
            input.EndTime);

        await timeBlockRepository.AddAsync(timeBlock, cancellationToken);

        return TimeBlockDto.FromDomain(timeBlock);
    }
}
