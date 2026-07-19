using HoraMarcada.Application.Common;
using HoraMarcada.Domain.Entities;

namespace HoraMarcada.Application.Models;

public record CategoryDto(
    Guid Id,
    Guid StoreId,
    string Name)
{
    public static CategoryDto FromDomain(Category category) => new(
        category.Id,
        category.StoreId,
        category.Name);
}

public record CreateCategoryInput(
    [property: SwaggerExample("Cortes")] string Name);
