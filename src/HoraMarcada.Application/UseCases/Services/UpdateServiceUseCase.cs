using HoraMarcada.Application.Exceptions;
using HoraMarcada.Application.Models;
using HoraMarcada.Application.Policies;
using HoraMarcada.Application.Repositories;
using HoraMarcada.Domain.Exceptions;

namespace HoraMarcada.Application.UseCases.Services;

public class UpdateServiceUseCase(
    IServiceRepository serviceRepository,
    IProfessionalRepository professionalRepository,
    ICategoryRepository categoryRepository,
    IStoreOwnershipPolicy ownershipPolicy)
{
    public async Task<ServiceDto> ExecuteAsync(Guid userId, Guid storeId, Guid serviceId, UpdateServiceInput input, CancellationToken cancellationToken = default)
    {
        await ownershipPolicy.EnsureUserOwnsStoreAsync(userId, storeId, cancellationToken);

        var service = await serviceRepository.GetByIdAsync(serviceId, cancellationToken);
        if (service == null || service.StoreId != storeId)
        {
            throw new NotFoundException($"Service with ID {serviceId} not found in store {storeId}.");
        }

        if (input.CategoryId == Guid.Empty)
        {
            throw new DomainValidationException("Category id is required.");
        }

        var category = await categoryRepository.GetByIdAsync(input.CategoryId, cancellationToken);
        if (category == null || category.StoreId != storeId)
        {
            throw new DomainValidationException($"Category {input.CategoryId} does not belong to store {storeId}.");
        }

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

        service.Update(input.Name, input.DurationMinutes, input.Price, input.Active);
        service.ChangeCategory(input.CategoryId);

        service.ClearProfessionals();
        if (input.ProfessionalIds != null)
        {
            foreach (var profId in input.ProfessionalIds)
            {
                service.AssignProfessional(profId);
            }
        }

        await serviceRepository.UpdateAsync(service, cancellationToken);
        return ServiceDto.FromDomain(service, category.Name);
    }
}
