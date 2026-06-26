using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HoraMarcada.Application.Common;
using HoraMarcada.Application.Repositories;
using HoraMarcada.Domain.Entities;
using HoraMarcada.Domain.Enums;
using Microsoft.Extensions.Logging;

namespace HoraMarcada.Infrastructure.Jobs;

public class SendDailySummaryJob(
    IProfessionalRepository professionalRepository,
    IAppointmentRepository appointmentRepository,
    IServiceRepository serviceRepository,
    IStoreRepository storeRepository,
    INotificationService notificationService,
    ILogger<SendDailySummaryJob> logger)
{
    public async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("Starting SendDailySummaryJob execution.");

        var professionals = await professionalRepository.ListAllActiveAsync(cancellationToken);
        logger.LogInformation("Found {Count} active professionals to process.", professionals.Count);

        var sortedProfessionals = professionals.OrderBy(p => p.Name).ToList();

        var brazilTimeZone = TimeZoneInfo.FindSystemTimeZoneById("America/Sao_Paulo");
        var nowBrazil = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, brazilTimeZone);
        var today = new DateTimeOffset(nowBrazil.Date, TimeSpan.FromHours(-3));

        var sentCount = 0;
        for (int i = 0; i < sortedProfessionals.Count; i++)
        {
            var professional = sortedProfessionals[i];

            if (professional.WhatsApp is null)
            {
                logger.LogWarning("Professional {Name} ({Id}) has no WhatsApp registered. Skipping.", professional.Name, professional.Id);
                continue;
            }

            try
            {
                var store = await storeRepository.GetByIdAsync(professional.StoreId, cancellationToken);
                if (store is null || !store.Active)
                {
                    logger.LogInformation("Store for professional {Name} is null or inactive. Skipping.", professional.Name);
                    continue;
                }

                var allAppointments = await appointmentRepository.ListByProfessionalAndDateAsync(
                    professional.Id,
                    today,
                    cancellationToken);

                var activeAppointments = allAppointments
                    .Where(a => a.Status == AppointmentStatus.Confirmed || 
                                a.Status == AppointmentStatus.ConfirmedByUser ||
                                a.Status == AppointmentStatus.Completed)
                    .ToList();

                if (activeAppointments.Count == 0)
                {
                    logger.LogInformation("Professional {Name} has no appointments for today ({Date}). Skipping daily summary.", professional.Name, today.ToString("yyyy-MM-dd"));
                    continue;
                }

                if (sentCount > 0)
                {
                    var delaySeconds = Random.Shared.Next(30, 60);
                    logger.LogInformation("Waiting {Delay} seconds before processing professional {Name} to avoid WhatsApp spam detection.", delaySeconds, professional.Name);
                    await Task.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken);
                }

                var services = await serviceRepository.ListByStoreIdAsync(professional.StoreId, cancellationToken);

                await notificationService.SendDailySummaryToProfessionalAsync(
                    professional,
                    store,
                    activeAppointments,
                    services,
                    cancellationToken);

                sentCount++;
                logger.LogInformation("Daily summary sent successfully to professional {Name}.", professional.Name);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error sending daily summary to professional {Name} ({Id}).", professional.Name, professional.Id);
            }
        }

        logger.LogInformation("Finished SendDailySummaryJob execution.");
    }
}
