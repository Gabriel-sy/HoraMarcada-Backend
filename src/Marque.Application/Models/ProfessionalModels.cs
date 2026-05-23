using Marque.Application.Common;

namespace Marque.Application.Models;

public record WeeklyHoursInput(
    [property: SwaggerExample("1")] int DayOfWeek,
    [property: SwaggerExample("09:00:00")] TimeOnly StartTime,
    [property: SwaggerExample("18:00:00")] TimeOnly EndTime,
    [property: SwaggerExample("12:00:00")] TimeOnly? LunchStart = null,
    [property: SwaggerExample("13:00:00")] TimeOnly? LunchEnd = null,
    [property: SwaggerExample("30")] short IntervalMinutes = 30);

public record AddProfessionalInput(
    [property: SwaggerExample("3fa85f64-5717-4562-b3fc-2c963f66afa6")] Guid UserId,
    [property: SwaggerExample("Jane Doe")] string Name,
    [property: SwaggerExample("https://example.com/jane.png")] string? PhotoUrl = null,
    [property: SwaggerExample("+5511988888888")] string? WhatsApp = null,
    List<WeeklyHoursInput>? WeeklyHours = null);

public record UpdateProfessionalInput(
    [property: SwaggerExample("3fa85f64-5717-4562-b3fc-2c963f66afa6")] Guid UserId,
    [property: SwaggerExample("Jane Smith")] string Name,
    [property: SwaggerExample("https://example.com/janesmith.png")] string? PhotoUrl = null,
    [property: SwaggerExample("+5511988888888")] string? WhatsApp = null,
    List<WeeklyHoursInput>? WeeklyHours = null);
