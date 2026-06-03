using HoraMarcada.Application.Models;
using HoraMarcada.Domain.Entities;

namespace HoraMarcada.Application.Repositories;

public interface ICustomerRepository
{
    Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<Customer?> GetByWhatsAppAsync(Guid storeId, string whatsapp, CancellationToken cancellationToken = default);

    Task AddAsync(Customer customer, CancellationToken cancellationToken = default);

    Task<(IReadOnlyList<CustomerListDto> Items, int TotalCount)> ListWithStatsByStoreAsync(
        Guid storeId,
        string? search = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default);

    Task<CustomerDetailDto?> GetDetailByIdAsync(
        Guid storeId,
        Guid customerId,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(Customer customer, CancellationToken cancellationToken = default);

    Task DeleteAsync(Customer customer, CancellationToken cancellationToken = default);
}
