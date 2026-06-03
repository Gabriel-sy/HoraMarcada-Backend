using HoraMarcada.Application.Exceptions;
using HoraMarcada.Application.Models;
using HoraMarcada.Application.Policies;
using HoraMarcada.Application.Repositories;

namespace HoraMarcada.Application.UseCases.Customers;

public class GetCustomerUseCase(
    ICustomerRepository customerRepository,
    IStoreOwnershipPolicy ownershipPolicy)
{
    public async Task<CustomerDetailDto> ExecuteAsync(
        Guid userId,
        Guid storeId,
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        await ownershipPolicy.EnsureUserOwnsStoreAsync(userId, storeId, cancellationToken);

        var detail = await customerRepository.GetDetailByIdAsync(storeId, customerId, cancellationToken);
        if (detail == null)
        {
            throw new NotFoundException($"Customer with ID {customerId} not found in store {storeId}.");
        }

        return detail;
    }
}
