using Marque.Application.Models;
using Marque.Application.Policies;
using Marque.Application.Repositories;
using Marque.Domain.Entities;
using Marque.Domain.Exceptions;

namespace Marque.Application.UseCases.Services;

public class AddServiceUseCase(
    IServiceRepository serviceRepository,
    IProfessionalRepository professionalRepository,
    IStoreOwnershipPolicy ownershipPolicy)
{
    public async Task<Service> ExecuteAsync(Guid userId, Guid storeId, AddServiceInput input, CancellationToken cancellationToken = default)
    {
        await ownershipPolicy.EnsureUserOwnsStoreAsync(userId, storeId, cancellationToken);

        if (input.ProfessionalIds != null && input.ProfessionalIds.Count > 0)
        {
            foreach (var profId in input.ProfessionalIds)
            {
                var professional = await professionalRepository.GetByIdAsync(profId, cancellationToken);
                if (professional == null || professional.StoreId != storeId)
                {
                    throw new DomainValidationException($"Professional {profId} does not belong to store {storeId}.");
                }
            }
        }

        var service = Service.Create(
            storeId,
            input.Name,
            input.DurationMinutes,
            input.Price);

        if (input.ProfessionalIds != null)
        {
            foreach (var profId in input.ProfessionalIds)
            {
                service.AssignProfessional(profId);
            }
        }

        await serviceRepository.AddAsync(service, cancellationToken);
        return service;
    }
}
