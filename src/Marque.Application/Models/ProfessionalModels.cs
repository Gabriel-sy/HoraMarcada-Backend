namespace Marque.Application.Models;

public record WeeklyHoursInput(
    int DayOfWeek,
    TimeOnly StartTime,
    TimeOnly EndTime,
    TimeOnly? LunchStart = null,
    TimeOnly? LunchEnd = null,
    short IntervalMinutes = 30);

public record AddProfessionalInput(
    Guid UserId,
    Guid StoreId,
    string Name,
    string? PhotoUrl = null,
    string? WhatsApp = null,
    List<WeeklyHoursInput>? WeeklyHours = null);

public record UpdateProfessionalInput(
    Guid UserId,
    Guid StoreId,
    Guid ProfessionalId,
    string Name,
    string? PhotoUrl = null,
    string? WhatsApp = null,
    List<WeeklyHoursInput>? WeeklyHours = null);
