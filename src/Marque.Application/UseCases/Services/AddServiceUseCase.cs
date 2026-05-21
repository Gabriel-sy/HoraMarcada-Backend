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
    public async Task<Service> ExecuteAsync(AddServiceInput input, CancellationToken cancellationToken = default)
    {
        await ownershipPolicy.EnsureUserOwnsStoreAsync(input.UserId, input.StoreId, cancellationToken);

        if (input.ProfessionalIds != null && input.ProfessionalIds.Count > 0)
        {
            foreach (var profId in input.ProfessionalIds)
            {
                var professional = await professionalRepository.GetByIdAsync(profId, cancellationToken);
                if (professional == null || professional.StoreId != input.StoreId)
                {
                    throw new DomainValidationException($"Professional {profId} does not belong to store {input.StoreId}.");
                }
            }
        }

        var service = Service.Create(
            input.StoreId,
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
