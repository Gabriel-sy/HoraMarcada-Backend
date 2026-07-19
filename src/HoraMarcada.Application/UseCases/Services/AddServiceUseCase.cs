using HoraMarcada.Application.Models;
using HoraMarcada.Application.Policies;
using HoraMarcada.Application.Repositories;
using HoraMarcada.Domain.Entities;
using HoraMarcada.Domain.Exceptions;

namespace HoraMarcada.Application.UseCases.Services;

public class AddServiceUseCase(
    IServiceRepository serviceRepository,
    IProfessionalRepository professionalRepository,
    ICategoryRepository categoryRepository,
    IStoreOwnershipPolicy ownershipPolicy)
{
    public async Task<ServiceDto> ExecuteAsync(Guid userId, Guid storeId, AddServiceInput input, CancellationToken cancellationToken = default)
    {
        await ownershipPolicy.EnsureUserOwnsStoreAsync(userId, storeId, cancellationToken);

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

        var service = Service.Create(
            storeId,
            input.CategoryId,
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
        return ServiceDto.FromDomain(service);
    }
}
