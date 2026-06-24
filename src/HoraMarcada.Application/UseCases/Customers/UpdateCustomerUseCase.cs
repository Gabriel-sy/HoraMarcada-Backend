using HoraMarcada.Application.Exceptions;
using HoraMarcada.Application.Models;
using HoraMarcada.Application.Policies;
using HoraMarcada.Application.Repositories;
using HoraMarcada.Domain.Entities;

namespace HoraMarcada.Application.UseCases.Customers;

public class UpdateCustomerUseCase(
    ICustomerRepository customerRepository,
    IStoreOwnershipPolicy ownershipPolicy)
{
    public async Task<Customer> ExecuteAsync(
        Guid userId,
        Guid storeId,
        Guid customerId,
        UpdateCustomerInput input,
        CancellationToken cancellationToken = default)
    {
        await ownershipPolicy.EnsureUserOwnsStoreAsync(userId, storeId, cancellationToken);

        var customer = await customerRepository.GetByIdAsync(customerId, cancellationToken);
        if (customer == null || customer.StoreId != storeId)
        {
            throw new NotFoundException($"Customer with ID {customerId} not found in store {storeId}.");
        }

        var existingWithWhatsApp = await customerRepository.GetByWhatsAppAsync(storeId, input.WhatsApp, cancellationToken);
        if (existingWithWhatsApp != null && existingWithWhatsApp.Id != customerId)
        {
            throw new ConflictException($"A customer with WhatsApp {input.WhatsApp} already exists in this store.");
        }

        customer.Update(input.Name, input.WhatsApp);
        await customerRepository.UpdateAsync(customer, cancellationToken);

        return customer;
    }
}
