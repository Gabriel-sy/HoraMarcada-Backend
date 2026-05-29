using HoraMarcada.Application.Common;
using HoraMarcada.Application.Exceptions;
using HoraMarcada.Application.Repositories;
using HoraMarcada.Domain.Entities;
using HoraMarcada.Domain.Enums;
using HoraMarcada.Domain.Exceptions;

namespace HoraMarcada.Application.UseCases.Availability;

public class GetAvailableSlotsUseCase(
    IStoreRepository storeRepository,
    IServiceRepository serviceRepository,
    IProfessionalRepository professionalRepository,
    IAppointmentRepository appointmentRepository,
    ITimeBlockRepository timeBlockRepository,
    ITimeProvider timeProvider,
    IAppConfiguration appConfiguration)
{
    public async Task<IReadOnlyList<DateTimeOffset>> ExecuteAsync(
        Guid storeId,
        Guid serviceId,
        DateTimeOffset date,
        Guid? professionalId = null,
        CancellationToken cancellationToken = default)
    {
        var store = await storeRepository.GetByIdAsync(storeId, cancellationToken);
        if (store == null)
            throw new NotFoundException($"Store with ID {storeId} not found.");

        var service = await serviceRepository.GetByIdAsync(serviceId, cancellationToken);
        if (service == null || service.StoreId != storeId)
            throw new NotFoundException($"Service with ID {serviceId} not found in store {storeId}.");

        if (!service.Active)
            return Array.Empty<DateTimeOffset>();

        List<Guid> profIdsToQuery = [];
        if (professionalId.HasValue)
        {
            var prof = await professionalRepository.GetByIdAsync(professionalId.Value, cancellationToken);
            if (prof == null || prof.StoreId != storeId || !prof.Active)
                throw new NotFoundException($"Professional with ID {professionalId.Value} not found or inactive in store {storeId}.");

            if (!service.ProfessionalIds.Contains(professionalId.Value))
                throw new DomainValidationException($"Professional {professionalId.Value} is not assigned to service {serviceId}.");

            profIdsToQuery.Add(professionalId.Value);
        }
        else
        {
            foreach (var pid in service.ProfessionalIds)
            {
                var prof = await professionalRepository.GetByIdAsync(pid, cancellationToken);
                if (prof != null && prof.Active && prof.StoreId == storeId)
                {
                    profIdsToQuery.Add(pid);
                }
            }
        }

        if (profIdsToQuery.Count == 0)
            return Array.Empty<DateTimeOffset>();

        var allSlots = new HashSet<DateTimeOffset>();
        var dayOfWeekVal = (int)date.DayOfWeek;

        foreach (var pid in profIdsToQuery)
        {
            var professional = await professionalRepository.GetByIdAsync(pid, cancellationToken);
            if (professional == null) continue;

            var schedule = professional.WeeklySchedules.FirstOrDefault(h => h.DayOfWeek == dayOfWeekVal);
            if (schedule == null)
                continue;

            var appointments = await appointmentRepository.ListByProfessionalAndDateAsync(pid, date, cancellationToken);
            var blocks = await timeBlockRepository.ListByProfessionalAndDateAsync(pid, date, cancellationToken);

            var current = schedule.StartTime;
            var endTime = schedule.EndTime;
            var interval = schedule.IntervalMinutes;
            var serviceDuration = service.DurationMinutes;

            while (true)
            {
                if (current.AddMinutes(serviceDuration) > endTime)
                    break;

                var slotStart = ToDateTimeOffset(date, current);
                var slotEnd = slotStart.AddMinutes(serviceDuration);

                bool overlapsLunch = false;
                if (schedule.LunchStart.HasValue && schedule.LunchEnd.HasValue)
                {
                    var lunchStart = ToDateTimeOffset(date, schedule.LunchStart.Value);
                    var lunchEnd = ToDateTimeOffset(date, schedule.LunchEnd.Value);

                    if (slotStart < lunchEnd && slotEnd > lunchStart)
                    {
                        overlapsLunch = true;
                    }
                }

                var minTime = timeProvider.UtcNow.AddMinutes(appConfiguration.MinimumSlotLeadTimeMinutes);
                if (!overlapsLunch && slotStart >= minTime)
                {
                    bool overlapsBlock = false;
                    foreach (var block in blocks)
                    {
                        if (slotStart < block.EndTime && slotEnd > block.StartTime)
                        {
                            overlapsBlock = true;
                            break;
                        }
                    }

                    if (!overlapsBlock)
                    {
                        bool overlapsAppointment = false;
                        foreach (var app in appointments)
                        {
                            if (app.Status != AppointmentStatus.Canceled)
                            {
                                if (slotStart < app.EndTime && slotEnd > app.StartTime)
                                {
                                    overlapsAppointment = true;
                                    break;
                                }
                            }
                        }

                        if (!overlapsAppointment)
                        {
                            allSlots.Add(slotStart);
                        }
                    }
                }

                var next = current.AddMinutes(interval);
                if (next <= current)
                    break;
                current = next;
            }
        }

        return allSlots.OrderBy(s => s).ToList();
    }

    private static DateTimeOffset ToDateTimeOffset(DateTimeOffset date, TimeOnly time)
    {
        return new DateTimeOffset(date.Year, date.Month, date.Day, time.Hour, time.Minute, time.Second, date.Offset);
    }
}
