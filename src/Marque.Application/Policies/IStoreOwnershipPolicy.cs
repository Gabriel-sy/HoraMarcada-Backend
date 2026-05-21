namespace Marque.Application.Policies;

public interface IStoreOwnershipPolicy
{
    Task EnsureUserOwnsStoreAsync(Guid userId, Guid storeId, CancellationToken cancellationToken = default);
}
