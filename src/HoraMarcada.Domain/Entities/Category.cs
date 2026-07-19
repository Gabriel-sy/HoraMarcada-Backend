using HoraMarcada.Domain.Exceptions;

namespace HoraMarcada.Domain.Entities;

public sealed class Category
{
    public const int MaxNameLength = 120;

    public Guid Id { get; private set; }
    public Guid StoreId { get; private set; }
    public string Name { get; private set; } = string.Empty;

    private Category()
    {
    }

    public static Category Create(Guid storeId, string name)
    {
        if (storeId == Guid.Empty)
            throw new DomainValidationException("Store id is required.");

        if (string.IsNullOrWhiteSpace(name))
            throw new DomainValidationException("Category name is required.");

        var trimmed = name.Trim();
        if (trimmed.Length > MaxNameLength)
            throw new DomainValidationException($"Category name cannot exceed {MaxNameLength} characters.");

        return new Category
        {
            Id = Guid.NewGuid(),
            StoreId = storeId,
            Name = trimmed
        };
    }
}
