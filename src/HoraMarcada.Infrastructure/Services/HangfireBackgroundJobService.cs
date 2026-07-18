using Hangfire;
using HoraMarcada.Application.Common.Interfaces;
using HoraMarcada.Infrastructure.Jobs;

namespace HoraMarcada.Infrastructure.Services;

public class HangfireBackgroundJobService(IBackgroundJobClient backgroundJobClient) : IBackgroundJobService
{
    public void ScheduleReminder(Guid appointmentId, DateTimeOffset executionTime)
    {
        backgroundJobClient.Schedule<SendReminderJob>(
            job => job.ExecuteAsync(appointmentId, CancellationToken.None),
            executionTime);
    }
}
