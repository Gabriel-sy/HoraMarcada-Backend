using HoraMarcada.Application.Exceptions;
using HoraMarcada.Application.Models;
using HoraMarcada.Application.Policies;
using HoraMarcada.Application.Repositories;

namespace HoraMarcada.Application.UseCases.Professionals;

public class UpdateProfessionalUseCase(
    IProfessionalRepository professionalRepository,
    IStoreOwnershipPolicy ownershipPolicy)
{
    public async Task<ProfessionalDto> ExecuteAsync(Guid userId, Guid storeId, Guid professionalId, UpdateProfessionalInput input, CancellationToken cancellationToken = default)
    {
        await ownershipPolicy.EnsureUserOwnsStoreAsync(userId, storeId, cancellationToken);

        var professional = await professionalRepository.GetByIdAsync(professionalId, cancellationToken);
        if (professional == null || professional.StoreId != storeId)
        {
            throw new NotFoundException($"Professional with ID {professionalId} not found in store {storeId}.");
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
        return ProfessionalDto.FromDomain(professional);
    }
}
