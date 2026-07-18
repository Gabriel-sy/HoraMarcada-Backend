namespace HoraMarcada.Application.Common.Interfaces;

public interface IBackgroundJobService
{
    void ScheduleReminder(Guid appointmentId, DateTimeOffset executionTime);
}
