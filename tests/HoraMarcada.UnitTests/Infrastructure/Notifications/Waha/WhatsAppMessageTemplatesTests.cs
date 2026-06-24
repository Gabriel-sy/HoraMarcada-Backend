using HoraMarcada.Domain.Entities;
using HoraMarcada.Infrastructure.Notifications.Waha;
using Xunit;

namespace HoraMarcada.UnitTests.Infrastructure.Notifications.Waha;

public class WhatsAppMessageTemplatesTests
{
    [Fact]
    public void BookingConfirmationToCustomer_ShouldIncludeAppointmentDetails()
    {
        var storeId = Guid.NewGuid();
        var store = Store.Create(storeId, "Barbearia", "barbearia", "+5511999999999", "store@example.com", address: "Rua A, 1");
        var service = Service.Create(storeId, "Corte", 30);
        var professional = Professional.Create(storeId, "João");
        var appointment = Appointment.Create(
            storeId,
            professional.Id,
            service.Id,
            "Maria",
            "+5511988887777",
            new DateTimeOffset(2026, 6, 10, 14, 0, 0, TimeSpan.FromHours(-3)),
            new DateTimeOffset(2026, 6, 10, 14, 30, 0, TimeSpan.FromHours(-3)),
            cancelToken: "abc123");

        var message = WhatsAppMessageTemplates.BookingConfirmationToCustomer(
            appointment,
            store,
            service,
            professional,
            "https://horamarcada.com/c/abc123");

        Assert.Contains("Maria", message);
        Assert.Contains("Corte", message);
        Assert.Contains("João", message);
        Assert.Contains("10/06/2026", message);
        Assert.Contains("14:00", message);
        Assert.Contains("*Cancelar agendamento:*", message);
        Assert.Contains("horamarcada.com/c/abc123", message);
        Assert.DoesNotContain("https://", message);
    }

    [Fact]
    public void BookingReminder24h_ShouldIncludeCancelAndRescheduleLinks()
    {
        var storeId = Guid.NewGuid();
        var store = Store.Create(storeId, "Barbearia", "barbearia", "+5511999999999", "store@example.com");
        var service = Service.Create(storeId, "Corte", 30);
        var professional = Professional.Create(storeId, "João");
        var appointment = Appointment.Create(
            storeId,
            professional.Id,
            service.Id,
            "Maria",
            "+5511988887777",
            new DateTimeOffset(2026, 6, 10, 14, 0, 0, TimeSpan.FromHours(-3)),
            new DateTimeOffset(2026, 6, 10, 14, 30, 0, TimeSpan.FromHours(-3)),
            cancelToken: "abc123");

        var message = WhatsAppMessageTemplates.BookingReminder24h(
            appointment,
            service,
            professional,
            "https://horamarcada.com/c/abc123",
            "https://horamarcada.com/r/abc123");

        Assert.Contains("*Cancelar agendamento:*", message);
        Assert.Contains("horamarcada.com/c/abc123", message);
        Assert.Contains("*Reagendar:*", message);
        Assert.Contains("horamarcada.com/r/abc123", message);
    }
}
