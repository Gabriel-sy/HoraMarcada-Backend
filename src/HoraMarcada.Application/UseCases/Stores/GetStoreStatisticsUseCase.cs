using HoraMarcada.Application.Models;
using HoraMarcada.Application.Policies;
using HoraMarcada.Application.Repositories;
using HoraMarcada.Domain.Entities;
using HoraMarcada.Domain.Enums;

namespace HoraMarcada.Application.UseCases.Stores;

public class GetStoreStatisticsUseCase(
    IAppointmentRepository appointmentRepository,
    IServiceRepository serviceRepository,
    IProfessionalRepository professionalRepository,
    IStoreOwnershipPolicy ownershipPolicy)
{
    public async Task<StoreStatisticsResponse> ExecuteAsync(
        Guid userId,
        GetStoreStatisticsInput input,
        CancellationToken cancellationToken = default)
    {
        await ownershipPolicy.EnsureUserOwnsStoreAsync(userId, input.StoreId, cancellationToken);

        var appointmentsInPeriod = await appointmentRepository.ListByStoreAsync(
            input.StoreId,
            input.StartDate,
            input.EndDate,
            cancellationToken: cancellationToken
        );

        var nonCanceledAppointments = appointmentsInPeriod
            .Where(a => a.Status != AppointmentStatus.Canceled)
            .ToList();

        var services = await serviceRepository.ListByStoreIdAsync(input.StoreId, cancellationToken);
        var professionals = await professionalRepository.ListByStoreIdAsync(input.StoreId, cancellationToken);

        var servicesMap = services.ToDictionary(s => s.Id);
        var professionalsMap = professionals.ToDictionary(p => p.Id);

        var estimatedRevenue = 0m;
        foreach (var app in nonCanceledAppointments)
        {
            if (servicesMap.TryGetValue(app.ServiceId, out var service) && service.Price != null)
            {
                estimatedRevenue += service.Price.Amount;
            }
        }

        var totalAppointments = nonCanceledAppointments.Count;

        var mostPopularServices = nonCanceledAppointments
            .GroupBy(a => a.ServiceId)
            .Select(g =>
            {
                var serviceName = servicesMap.TryGetValue(g.Key, out var s) ? s.Name : "Serviço Removido";
                var count = g.Count();
                var revenue = g.Sum(a => (servicesMap.TryGetValue(a.ServiceId, out var s) && s.Price != null) ? s.Price.Amount : 0m);
                return new ServicePopularityDto(serviceName, count, revenue);
            })
            .OrderByDescending(s => s.Count)
            .ToList();

        var professionalOccupation = nonCanceledAppointments
            .GroupBy(a => a.ProfessionalId)
            .Select(g =>
            {
                var profName = professionalsMap.TryGetValue(g.Key, out var p) ? p.Name : "Profissional Removido";
                var count = g.Count();
                var totalMinutes = (int)g.Sum(a => (a.EndTime - a.StartTime).TotalMinutes);
                return new ProfessionalOccupationDto(profName, count, totalMinutes);
            })
            .OrderByDescending(p => p.Count)
            .ToList();

        var clientOffset = input.EndDate.Offset;
        var endOfTodayClient = new DateTimeOffset(input.EndDate.Year, input.EndDate.Month, input.EndDate.Day, 23, 59, 59, clientOffset);
        var start15DaysAgoUtc = new DateTimeOffset(endOfTodayClient.AddDays(-14).Year, endOfTodayClient.AddDays(-14).Month, endOfTodayClient.AddDays(-14).Day, 0, 0, 0, clientOffset).ToUniversalTime();
        var endTodayUtc = endOfTodayClient.ToUniversalTime();

        var appointments15Days = await appointmentRepository.ListByStoreAsync(
            input.StoreId,
            start15DaysAgoUtc,
            endTodayUtc,
            cancellationToken: cancellationToken
        );

        var dailyStats = new Dictionary<string, int>();
        for (int i = 0; i < 15; i++)
        {
            var dateStr = endOfTodayClient.AddDays(-i).ToString("yyyy-MM-dd");
            dailyStats[dateStr] = 0;
        }

        foreach (var appointment in appointments15Days.Where(a => a.Status != AppointmentStatus.Canceled))
        {
            var localDateStr = appointment.StartTime.ToOffset(clientOffset).ToString("yyyy-MM-dd");
            if (dailyStats.ContainsKey(localDateStr))
            {
                dailyStats[localDateStr]++;
            }
        }

        var dailyAppointments = dailyStats
            .Select(kv => new DailyAppointmentDto(kv.Key, kv.Value))
            .OrderBy(dto => dto.Date)
            .ToList();

        return new StoreStatisticsResponse(
            estimatedRevenue,
            totalAppointments,
            mostPopularServices,
            professionalOccupation,
            dailyAppointments
        );
    }
}
