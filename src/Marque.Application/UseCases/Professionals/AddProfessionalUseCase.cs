using Marque.Application.Models;
using Marque.Application.Policies;
using Marque.Application.Repositories;
using Marque.Domain.Entities;

namespace Marque.Application.UseCases.Professionals;

public class AddProfessionalUseCase(
    IProfessionalRepository professionalRepository,
    IStoreOwnershipPolicy ownershipPolicy)
{
    public async Task<Professional> ExecuteAsync(AddProfessionalInput input, CancellationToken cancellationToken = default)
    {
        await ownershipPolicy.EnsureUserOwnsStoreAsync(input.UserId, input.StoreId, cancellationToken);

        var professional = Professional.Create(
            input.StoreId,
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
