using HoraMarcada.Domain.Entities;

namespace HoraMarcada.Application.Models;

public record CustomerDto(
    Guid Id,
    Guid StoreId,
    string Name,
    string WhatsApp,
    DateTimeOffset CreatedAt)
{
    public static CustomerDto FromDomain(Customer customer) => new(
        customer.Id,
        customer.StoreId,
        customer.Name,
        customer.WhatsApp.Value,
        customer.CreatedAt);
}

public record CustomerListDto(
    Guid Id,
    Guid StoreId,
    string Name,
    string WhatsApp,
    int TotalAppointments,
    DateTimeOffset? LastVisit,
    DateTimeOffset CreatedAt);

public record CustomerDetailDto(
    Guid Id,
    Guid StoreId,
    string Name,
    string WhatsApp,
    DateTimeOffset CreatedAt,
    IReadOnlyList<CustomerAppointmentHistoryDto> AppointmentHistory);

public record CustomerAppointmentHistoryDto(
    Guid Id,
    string ServiceName,
    decimal? Price,
    string ProfessionalName,
    DateTimeOffset StartTime,
    string Status);

public record UpdateCustomerInput(
    string Name,
    string WhatsApp);
