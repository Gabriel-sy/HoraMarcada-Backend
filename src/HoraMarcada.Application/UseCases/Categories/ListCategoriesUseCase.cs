using HoraMarcada.Application.Models;
using HoraMarcada.Application.Policies;
using HoraMarcada.Application.Repositories;

namespace HoraMarcada.Application.UseCases.Categories;

public class ListCategoriesUseCase(
    ICategoryRepository categoryRepository,
    IStoreOwnershipPolicy ownershipPolicy)
{
    public async Task<IReadOnlyList<CategoryDto>> ExecuteAsync(
        Guid userId,
        Guid storeId,
        CancellationToken cancellationToken = default)
    {
        await ownershipPolicy.EnsureUserOwnsStoreAsync(userId, storeId, cancellationToken);

        var categories = await categoryRepository.ListByStoreIdAsync(storeId, cancellationToken);
        return categories
            .Select(CategoryDto.FromDomain)
            .OrderBy(c => c.Name)
            .ToList();
    }
}
