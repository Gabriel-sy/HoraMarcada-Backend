using HoraMarcada.Application.Exceptions;
using HoraMarcada.Application.Models;
using HoraMarcada.Application.Policies;
using HoraMarcada.Application.Repositories;
using HoraMarcada.Domain.Entities;

namespace HoraMarcada.Application.UseCases.Categories;

public class CreateCategoryUseCase(
    ICategoryRepository categoryRepository,
    IStoreOwnershipPolicy ownershipPolicy)
{
    public async Task<CategoryDto> ExecuteAsync(
        Guid userId,
        Guid storeId,
        CreateCategoryInput input,
        CancellationToken cancellationToken = default)
    {
        await ownershipPolicy.EnsureUserOwnsStoreAsync(userId, storeId, cancellationToken);

        var existing = await categoryRepository.GetByNameAsync(storeId, input.Name, cancellationToken);
        if (existing != null)
        {
            throw new ConflictException($"A category named '{existing.Name}' already exists in this store.");
        }

        var category = Category.Create(storeId, input.Name);
        await categoryRepository.AddAsync(category, cancellationToken);
        return CategoryDto.FromDomain(category);
    }
}
