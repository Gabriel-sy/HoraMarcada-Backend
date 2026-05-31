using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace HoraMarcada.Infrastructure.Notifications;

public class NotificationQueueProcessor(
    INotificationQueue queue,
    IServiceScopeFactory scopeFactory,
    ILogger<NotificationQueueProcessor> logger) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                var job = await queue.DequeueAsync(stoppingToken);
                await ProcessJobWithRetryAsync(job, stoppingToken);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error occurred processing notification queue.");
            }
        }
    }

    private async Task ProcessJobWithRetryAsync(NotificationJob job, CancellationToken stoppingToken)
    {
        const int maxRetries = 3;
        const int initialDelaySeconds = 2;

        for (int attempt = 1; attempt <= maxRetries; attempt++)
        {
            try
            {
                using var scope = scopeFactory.CreateScope();
                var emailService = scope.ServiceProvider.GetRequiredService<ResendEmailNotificationService>();

                switch (job.Type)
                {
                    case NotificationJobType.BookingConfirmationToCustomer:
                        await emailService.SendBookingConfirmationToCustomerAsync(job.Appointment, job.Store, job.Service, job.Professional, stoppingToken);
                        break;
                    case NotificationJobType.BookingNotificationToProfessional:
                        await emailService.SendBookingNotificationToProfessionalAsync(job.Appointment, job.Store, job.Service, job.Professional, stoppingToken);
                        break;
                    case NotificationJobType.BookingNotificationToStore:
                        await emailService.SendBookingNotificationToStoreAsync(job.Appointment, job.Store, job.Service, job.Professional, stoppingToken);
                        break;
                    case NotificationJobType.CancellationNoticeToCustomer:
                        await emailService.SendCancellationNoticeToCustomerAsync(job.Appointment, job.Store, job.Service, job.Professional, stoppingToken);
                        break;
                    case NotificationJobType.CancellationNotificationToProfessional:
                        await emailService.SendCancellationNotificationToProfessionalAsync(job.Appointment, job.Store, job.Service, job.Professional, stoppingToken);
                        break;
                    case NotificationJobType.CancellationNotificationToStore:
                        await emailService.SendCancellationNotificationToStoreAsync(job.Appointment, job.Store, job.Service, job.Professional, stoppingToken);
                        break;
                    case NotificationJobType.BookingReminder24h:
                        await emailService.SendBookingReminder24hAsync(job.Appointment, job.Store, job.Service, job.Professional, stoppingToken);
                        break;
                }

                return;
            }
            catch (Exception ex) when (attempt < maxRetries && !stoppingToken.IsCancellationRequested)
            {
                var delay = initialDelaySeconds * attempt;
                logger.LogWarning(ex, "Failed to send notification (Attempt {Attempt}/{MaxRetries}). Retrying in {Delay} seconds...", attempt, maxRetries, delay);
                await Task.Delay(TimeSpan.FromSeconds(delay), stoppingToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to send notification after {MaxRetries} attempts.", maxRetries);
            }
        }
    }
}
