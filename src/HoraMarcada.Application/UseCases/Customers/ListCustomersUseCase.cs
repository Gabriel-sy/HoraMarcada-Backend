using HoraMarcada.Application.Models;
using HoraMarcada.Application.Policies;
using HoraMarcada.Application.Repositories;

namespace HoraMarcada.Application.UseCases.Customers;

public class ListCustomersUseCase(
    ICustomerRepository customerRepository,
    IStoreOwnershipPolicy ownershipPolicy)
{
    public async Task<PagedResult<CustomerListDto>> ExecuteAsync(
        Guid userId,
        Guid storeId,
        string? search,
        int page,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        await ownershipPolicy.EnsureUserOwnsStoreAsync(userId, storeId, cancellationToken);

        var (items, totalCount) = await customerRepository.ListWithStatsByStoreAsync(
            storeId,
            search,
            page,
            pageSize,
            cancellationToken);

        return new PagedResult<CustomerListDto>(items, totalCount, page, pageSize);
    }
}
