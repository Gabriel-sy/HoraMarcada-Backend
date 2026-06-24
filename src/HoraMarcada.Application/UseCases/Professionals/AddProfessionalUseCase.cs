using HoraMarcada.Application.Models;
using HoraMarcada.Application.Policies;
using HoraMarcada.Application.Repositories;
using HoraMarcada.Domain.Entities;

namespace HoraMarcada.Application.UseCases.Professionals;

public class AddProfessionalUseCase(
    IProfessionalRepository professionalRepository,
    IStoreOwnershipPolicy ownershipPolicy)
{
    public async Task<Professional> ExecuteAsync(Guid userId, Guid storeId, AddProfessionalInput input, CancellationToken cancellationToken = default)
    {
        await ownershipPolicy.EnsureUserOwnsStoreAsync(userId, storeId, cancellationToken);

        var professional = Professional.Create(
            storeId,
            input.Name,
            input.PhotoUrl,
            input.WhatsApp);

        if (input.WeeklyHours != null)
        {
            foreach (var h in input.WeeklyHours)
            {
                professional.AddWeeklyHours(
                    h.DayOfWeek,
                    h.StartTime,
                    h.EndTime,
                    h.LunchStart,
                    h.LunchEnd,
                    h.IntervalMinutes);
            }
        }

        await professionalRepository.AddAsync(professional, cancellationToken);
        return professional;
    }
}
