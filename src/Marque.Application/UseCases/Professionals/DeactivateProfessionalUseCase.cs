using Marque.Application.Exceptions;
using Marque.Application.Policies;
using Marque.Application.Repositories;

namespace Marque.Application.UseCases.Professionals;

public class DeactivateProfessionalUseCase(
    IProfessionalRepository professionalRepository,
    IStoreOwnershipPolicy ownershipPolicy)
{
    public async Task ExecuteAsync(Guid userId, Guid storeId, Guid professionalId, CancellationToken cancellationToken = default)
    {
        await ownershipPolicy.EnsureUserOwnsStoreAsync(userId, storeId, cancellationToken);

        var professional = await professionalRepository.GetByIdAsync(professionalId, cancellationToken);
        if (professional == null || professional.StoreId != storeId)
        {
            throw new NotFoundException($"Professional with ID {professionalId} not found in store {storeId}.");
        }

        professional.Deactivate();
        await professionalRepository.UpdateAsync(professional, cancellationToken);
    }
}
