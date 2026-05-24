using HoraMarcada.Application.Exceptions;
using HoraMarcada.Application.Repositories;
using HoraMarcada.Domain.Entities;

namespace HoraMarcada.Application.UseCases.Services;

public class ListProfessionalsForServiceUseCase(
    IServiceRepository serviceRepository,
    IProfessionalRepository professionalRepository)
{
    public async Task<IReadOnlyList<Professional>> ExecuteAsync(Guid serviceId, CancellationToken cancellationToken = default)
    {
        var service = await serviceRepository.GetByIdAsync(serviceId, cancellationToken);
        if (service == null)
        {
            throw new NotFoundException($"Service with ID {serviceId} not found.");
        }

        var professionals = new List<Professional>();
        foreach (var profId in service.ProfessionalIds)
        {
            var professional = await professionalRepository.GetByIdAsync(profId, cancellationToken);
            if (professional != null && professional.Active)
            {
                professionals.Add(professional);
            }
        }

        return professionals;
    }
}
