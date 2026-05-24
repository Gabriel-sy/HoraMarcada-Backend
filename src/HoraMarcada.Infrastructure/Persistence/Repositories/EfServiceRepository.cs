using HoraMarcada.Application.Repositories;
using HoraMarcada.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace HoraMarcada.Infrastructure.Persistence.Repositories;

internal sealed class EfServiceRepository(HoraMarcadaDbContext context) : IServiceRepository
{
    public async Task<Service?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var service = await context.Services
            .FirstOrDefaultAsync(s => s.Id == id, cancellationToken);

        if (service is null)
            return null;

        await LoadProfessionalIdsAsync(service, cancellationToken);
        return service;
    }

    public async Task<IReadOnlyList<Service>> ListByStoreIdAsync(
        Guid storeId,
        CancellationToken cancellationToken = default)
    {
        var services = await context.Services
            .Where(s => s.StoreId == storeId)
            .ToListAsync(cancellationToken);

        if (services.Count == 0)
            return services;

        var serviceIds = services.Select(s => s.Id).ToList();
        var associations = await context.ServiceProfessionals
            .Where(sp => serviceIds.Contains(sp.ServiceId))
            .ToListAsync(cancellationToken);

        foreach (var service in services)
        {
            foreach (var professionalId in associations
                         .Where(a => a.ServiceId == service.Id)
                         .Select(a => a.ProfessionalId))
            {
                service.AssignProfessional(professionalId);
            }
        }

        return services;
    }

    public async Task AddAsync(Service service, CancellationToken cancellationToken = default)
    {
        await context.Services.AddAsync(service, cancellationToken);

        foreach (var professionalId in service.ProfessionalIds)
        {
            await context.ServiceProfessionals.AddAsync(
                new ServiceProfessional
                {
                    ServiceId = service.Id,
                    ProfessionalId = professionalId
                },
                cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task UpdateAsync(Service service, CancellationToken cancellationToken = default)
    {
        context.Services.Update(service);

        // Update professional associations
        var existingAssociations = await context.ServiceProfessionals
            .Where(sp => sp.ServiceId == service.Id)
            .ToListAsync(cancellationToken);

        context.ServiceProfessionals.RemoveRange(existingAssociations);

        foreach (var professionalId in service.ProfessionalIds)
        {
            await context.ServiceProfessionals.AddAsync(
                new ServiceProfessional
                {
                    ServiceId = service.Id,
                    ProfessionalId = professionalId
                },
                cancellationToken);
        }

        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task LoadProfessionalIdsAsync(Service service, CancellationToken cancellationToken)
    {
        var professionalIds = await context.ServiceProfessionals
            .Where(sp => sp.ServiceId == service.Id)
            .Select(sp => sp.ProfessionalId)
            .ToListAsync(cancellationToken);

        foreach (var professionalId in professionalIds)
            service.AssignProfessional(professionalId);
    }
}
