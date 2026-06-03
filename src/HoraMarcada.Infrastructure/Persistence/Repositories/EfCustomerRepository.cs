using HoraMarcada.Application.Models;
using HoraMarcada.Application.Repositories;
using HoraMarcada.Domain.Entities;
using HoraMarcada.Domain.ValueObjects;
using Microsoft.EntityFrameworkCore;
using HoraMarcada.Infrastructure.Persistence.Extensions;

namespace HoraMarcada.Infrastructure.Persistence.Repositories;

internal sealed class EfCustomerRepository(HoraMarcadaDbContext context) : ICustomerRepository
{
    public async Task<Customer?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return await context.Customers.FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
    }

    public async Task<Customer?> GetByWhatsAppAsync(Guid storeId, string whatsapp, CancellationToken cancellationToken = default)
    {
        var phone = PhoneNumber.Create(whatsapp);
        return await context.Customers
            .FirstOrDefaultAsync(c => c.StoreId == storeId && c.WhatsApp == phone, cancellationToken);
    }

    public async Task AddAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        await context.Customers.AddAsync(customer, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<(IReadOnlyList<CustomerListDto> Items, int TotalCount)> ListWithStatsByStoreAsync(
        Guid storeId,
        string? search = null,
        int page = 1,
        int pageSize = 10,
        CancellationToken cancellationToken = default)
    {
        var query = context.Customers.Where(c => c.StoreId == storeId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var searchNormalized = search.RemoveDiacritics().Trim().ToLowerInvariant();
            query = query.Where(c => EF.Functions.ILike(EF.Functions.Unaccent(c.Name), $"%{searchNormalized}%") || ((string)(object)c.WhatsApp).Contains(searchNormalized));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var todayStart = new DateTimeOffset(DateTime.UtcNow.Date, TimeSpan.Zero);

        var items = await query
            .OrderBy(c => c.Name)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(c => new CustomerListDto(
                c.Id,
                c.StoreId,
                c.Name,
                c.WhatsApp.Value,
                c.Email == null ? null : c.Email.Value,
                context.Appointments.Count(a => a.CustomerId == c.Id && a.StartTime < todayStart && a.Status != Domain.Enums.AppointmentStatus.Canceled),
                context.Appointments
                    .Where(a => a.CustomerId == c.Id && a.StartTime < todayStart && a.Status != Domain.Enums.AppointmentStatus.Canceled)
                    .Max(a => (DateTimeOffset?)a.StartTime),
                c.CreatedAt
            ))
            .ToListAsync(cancellationToken);

        return (items, totalCount);
    }

    public async Task<CustomerDetailDto?> GetDetailByIdAsync(
        Guid storeId,
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        var customer = await context.Customers
            .FirstOrDefaultAsync(c => c.StoreId == storeId && c.Id == customerId, cancellationToken);

        if (customer == null)
        {
            return null;
        }

        var appointments = await (
            from a in context.Appointments
            where a.CustomerId == customerId
            join s in context.Services on a.ServiceId equals s.Id
            join p in context.Professionals on a.ProfessionalId equals p.Id
            orderby a.StartTime descending
            select new CustomerAppointmentHistoryDto(
                a.Id,
                s.Name,
                s.Price == null ? null : (decimal?)s.Price.Amount,
                p.Name,
                a.StartTime,
                a.Status.ToString().ToLowerInvariant()
            )
        ).ToListAsync(cancellationToken);

        return new CustomerDetailDto(
            customer.Id,
            customer.StoreId,
            customer.Name,
            customer.WhatsApp.Value,
            customer.Email == null ? null : customer.Email.Value,
            customer.CreatedAt,
            appointments
        );
    }

    public async Task UpdateAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        context.Customers.Update(customer);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task DeleteAsync(Customer customer, CancellationToken cancellationToken = default)
    {
        context.Customers.Remove(customer);
        await context.SaveChangesAsync(cancellationToken);
    }
}
