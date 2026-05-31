using System;

namespace HoraMarcada.Application.Common;

public interface IBackgroundJobService
{
    void ScheduleReminder(Guid appointmentId, DateTimeOffset executionTime);
}
