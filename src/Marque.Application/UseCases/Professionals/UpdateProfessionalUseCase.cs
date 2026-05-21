using Marque.Application.Exceptions;
using Marque.Application.Models;
using Marque.Application.Policies;
using Marque.Application.Repositories;
using Marque.Domain.Entities;

namespace Marque.Application.UseCases.Professionals;

public class UpdateProfessionalUseCase(
    IProfessionalRepository professionalRepository,
    IStoreOwnershipPolicy ownershipPolicy)
{
    public async Task<Professional> ExecuteAsync(UpdateProfessionalInput input, CancellationToken cancellationToken = default)
    {
        await ownershipPolicy.EnsureUserOwnsStoreAsync(input.UserId, input.StoreId, cancellationToken);

        var professional = await professionalRepository.GetByIdAsync(input.ProfessionalId, cancellationToken);
        if (professional == null || professional.StoreId != input.StoreId)
        {
            throw new NotFoundException($"Professional with ID {input.ProfessionalId} not found in store {input.StoreId}.");
        }

        professional.Update(input.Name, input.PhotoUrl, input.WhatsApp);

        professional.ClearWeeklyHours();
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

        await professionalRepository.UpdateAsync(professional, cancellationToken);
        return professional;
    }
}
