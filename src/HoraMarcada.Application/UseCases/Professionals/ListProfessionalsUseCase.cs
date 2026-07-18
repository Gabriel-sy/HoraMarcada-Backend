using HoraMarcada.Application.Models;
using HoraMarcada.Application.Repositories;

namespace HoraMarcada.Application.UseCases.Professionals;

public class ListProfessionalsUseCase(IProfessionalRepository professionalRepository)
{
    public async Task<IReadOnlyList<ProfessionalDto>> ExecuteAsync(Guid storeId, bool activeOnly, CancellationToken cancellationToken = default)
    {
        var list = await professionalRepository.ListByStoreIdAsync(storeId, cancellationToken);
        if (activeOnly)
        {
            return list.Where(p => p.Active).Select(ProfessionalDto.FromDomain).OrderBy(p => p.Name).ToList();
        }
        return list.Select(ProfessionalDto.FromDomain).OrderBy(p => p.Name).ToList();
    }
}
