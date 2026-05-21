using Marque.Application.Exceptions;
using Marque.Application.Models;
using Marque.Application.Policies;
using Marque.Application.Repositories;
using Marque.Domain.Entities;
using Marque.Domain.Exceptions;

namespace Marque.Application.UseCases.Services;

public class UpdateServiceUseCase(
    IServiceRepository serviceRepository,
    IProfessionalRepository professionalRepository,
    IStoreOwnershipPolicy ownershipPolicy)
{
    public async Task<Service> ExecuteAsync(UpdateServiceInput input, CancellationToken cancellationToken = default)
    {
        await ownershipPolicy.EnsureUserOwnsStoreAsync(input.UserId, input.StoreId, cancellationToken);

        var service = await serviceRepository.GetByIdAsync(input.ServiceId, cancellationToken);
        if (service == null || service.StoreId != input.StoreId)
        {
            throw new NotFoundException($"Service with ID {input.ServiceId} not found in store {input.StoreId}.");
        }

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

        service.Update(input.Name, input.DurationMinutes, input.Price, input.Active);

        service.ClearProfessionals();
        if (input.ProfessionalIds != null)
        {
            foreach (var profId in input.ProfessionalIds)
            {
                service.AssignProfessional(profId);
            }
        }

        await serviceRepository.UpdateAsync(service, cancellationToken);
        return service;
    }
}
