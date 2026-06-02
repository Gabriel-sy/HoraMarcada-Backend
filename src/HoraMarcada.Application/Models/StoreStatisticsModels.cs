using HoraMarcada.Application.Common;

namespace HoraMarcada.Application.Models;

public record GetStoreStatisticsInput(
    [property: SwaggerExample("3fa85f64-5717-4562-b3fc-2c963f66afa6")] Guid StoreId,
    [property: SwaggerExample("2026-05-18T00:00:00Z")] DateTimeOffset StartDate,
    [property: SwaggerExample("2026-06-01T23:59:59Z")] DateTimeOffset EndDate
);

public record StoreStatisticsResponse(
    [property: SwaggerExample("1250.00")] decimal EstimatedRevenue,
    [property: SwaggerExample("45")] int TotalAppointments,
    IReadOnlyList<ServicePopularityDto> MostPopularServices,
    IReadOnlyList<ProfessionalOccupationDto> ProfessionalOccupation,
    IReadOnlyList<DailyAppointmentDto> DailyAppointments
);

public record ServicePopularityDto(
    [property: SwaggerExample("Corte de Cabelo")] string ServiceName,
    [property: SwaggerExample("25")] int Count,
    [property: SwaggerExample("750.00")] decimal Revenue
);

public record ProfessionalOccupationDto(
    [property: SwaggerExample("João Barbeiro")] string ProfessionalName,
    [property: SwaggerExample("30")] int Count,
    [property: SwaggerExample("1800")] int TotalMinutes
);

public record DailyAppointmentDto(
    [property: SwaggerExample("2026-05-18")] string Date,
    [property: SwaggerExample("5")] int Count
);
