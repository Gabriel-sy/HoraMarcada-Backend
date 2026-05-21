using Marque.Application.Repositories;
using Marque.Domain.Entities;

namespace Marque.Application.UseCases.Professionals;

public class ListProfessionalsUseCase(IProfessionalRepository professionalRepository)
{
    public async Task<IReadOnlyList<Professional>> ExecuteAsync(Guid storeId, bool activeOnly, CancellationToken cancellationToken = default)
    {
        var list = await professionalRepository.ListByStoreIdAsync(storeId, cancellationToken);
        if (activeOnly)
        {
            return list.Where(p => p.Active).ToList();
        }
        return list;
    }
}
