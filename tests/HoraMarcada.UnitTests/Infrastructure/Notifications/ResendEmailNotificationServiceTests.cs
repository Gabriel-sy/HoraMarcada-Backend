using Moq;
using Xunit;
using HoraMarcada.Infrastructure.Notifications;
using HoraMarcada.Domain.Entities;
using HoraMarcada.Domain.ValueObjects;
using Resend;
using Microsoft.Extensions.Configuration;

namespace HoraMarcada.UnitTests.Infrastructure.Notifications;

public class ResendEmailNotificationServiceTests
{
    private readonly Mock<IResend> _resendMock;
    private readonly Mock<IConfiguration> _configurationMock;
    private readonly ResendEmailNotificationService _service;

    public ResendEmailNotificationServiceTests()
    {
        _resendMock = new Mock<IResend>();
        _configurationMock = new Mock<IConfiguration>();
        _configurationMock.Setup(c => c["Resend:FromEmail"]).Returns("onboarding@resend.dev");
        _service = new ResendEmailNotificationService(_resendMock.Object, _configurationMock.Object);
    }

    [Fact]
    public async Task SendBookingConfirmationToCustomerAsync_ShouldSendEmailToCustomerWithCta()
    {
        var storeId = Guid.NewGuid();
        var professionalId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var store = Store.Create(storeId, "Store Name", "store-slug", "+5511999999999", "store@example.com");
        var service = Service.Create(storeId, "Haircut", 30);
        var professional = Professional.Create(storeId, "Jane Doe", email: "jane@example.com");
        var idField = typeof(Professional).GetProperty("Id");
        idField?.SetValue(professional, professionalId);

        var appointment = Appointment.Create(
            storeId,
            professionalId,
            serviceId,
            "John Doe",
            "+5511988888888",
            "john@example.com",
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddMinutes(30),
            cancelToken: "cancel123"
        );

        _resendMock
            .Setup(r => r.EmailSendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResendResponse<Guid>(Guid.NewGuid(), null!));

        await _service.SendBookingConfirmationToCustomerAsync(appointment, store, service, professional);

        _resendMock.Verify(r => r.EmailSendAsync(It.Is<EmailMessage>(m =>
            m.From.Email == "onboarding@resend.dev" &&
            m.Subject.Contains("Agendamento Confirmado") &&
            m.HtmlBody.Contains("John Doe") &&
            m.HtmlBody.Contains("Haircut") &&
            m.HtmlBody.Contains("Jane Doe") &&
            m.To.Any(t => t.Email == "john@example.com") &&
            m.HtmlBody.Contains("/cancel/cancel123")
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendBookingConfirmationToCustomerAsync_ShouldFormatTimeInUtcMinus3()
    {
        var storeId = Guid.NewGuid();
        var professionalId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var store = Store.Create(storeId, "Store Name", "store-slug", "+5511999999999", "store@example.com");
        var service = Service.Create(storeId, "Haircut", 30);
        var professional = Professional.Create(storeId, "Jane Doe", email: "jane@example.com");
        var idField = typeof(Professional).GetProperty("Id");
        idField?.SetValue(professional, professionalId);

        var startTimeUtc = new DateTimeOffset(2026, 5, 27, 23, 0, 0, TimeSpan.Zero);
        var appointment = Appointment.Create(
            storeId,
            professionalId,
            serviceId,
            "John Doe",
            "+5511988888888",
            "john@example.com",
            startTimeUtc,
            startTimeUtc.AddMinutes(30),
            cancelToken: "cancel123"
        );

        _resendMock
            .Setup(r => r.EmailSendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResendResponse<Guid>(Guid.NewGuid(), null!));

        await _service.SendBookingConfirmationToCustomerAsync(appointment, store, service, professional);

        _resendMock.Verify(r => r.EmailSendAsync(It.Is<EmailMessage>(m =>
            m.HtmlBody.Contains("27/05/2026 20:00")
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendBookingNotificationToProfessionalAsync_ShouldSendEmailToProfessional()
    {
        var storeId = Guid.NewGuid();
        var professionalId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var store = Store.Create(storeId, "Store Name", "store-slug", "+5511999999999", "store@example.com");
        var service = Service.Create(storeId, "Haircut", 30);
        var professional = Professional.Create(storeId, "Jane Doe", email: "jane@example.com");
        var idField = typeof(Professional).GetProperty("Id");
        idField?.SetValue(professional, professionalId);

        var appointment = Appointment.Create(
            storeId,
            professionalId,
            serviceId,
            "John Doe",
            "+5511988888888",
            "john@example.com",
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddMinutes(30),
            cancelToken: "cancel123"
        );

        _resendMock
            .Setup(r => r.EmailSendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResendResponse<Guid>(Guid.NewGuid(), null!));

        await _service.SendBookingNotificationToProfessionalAsync(appointment, store, service, professional);

        _resendMock.Verify(r => r.EmailSendAsync(It.Is<EmailMessage>(m =>
            m.From.Email == "onboarding@resend.dev" &&
            m.Subject.Contains("Novo Agendamento") &&
            m.HtmlBody.Contains("Jane Doe") &&
            m.To.Any(t => t.Email == "jane@example.com") &&
            m.HtmlBody.Contains("John Doe") &&
            m.HtmlBody.Contains("/cancel/cancel123")
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendCancellationNoticeToCustomerAsync_ShouldSendCancellationNotice()
    {
        var storeId = Guid.NewGuid();
        var professionalId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var store = Store.Create(storeId, "Store Name", "store-slug", "+5511999999999", "store@example.com");
        var service = Service.Create(storeId, "Haircut", 30);
        var professional = Professional.Create(storeId, "Jane Doe", email: "jane@example.com");
        var idField = typeof(Professional).GetProperty("Id");
        idField?.SetValue(professional, professionalId);

        var appointment = Appointment.Create(
            storeId,
            professionalId,
            serviceId,
            "John Doe",
            "+5511988888888",
            "john@example.com",
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddMinutes(30)
        );

        _resendMock
            .Setup(r => r.EmailSendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResendResponse<Guid>(Guid.NewGuid(), null!));

        await _service.SendCancellationNoticeToCustomerAsync(appointment, store, service, professional);

        _resendMock.Verify(r => r.EmailSendAsync(It.Is<EmailMessage>(m =>
            m.From.Email == "onboarding@resend.dev" &&
            m.Subject.Contains("Agendamento Cancelado") &&
            m.To.Any(t => t.Email == "john@example.com")
        ), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendCancellationNotificationToProfessionalAsync_ShouldSendCancellationNotificationToProfessional()
    {
        var storeId = Guid.NewGuid();
        var professionalId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var store = Store.Create(storeId, "Store Name", "store-slug", "+5511999999999", "store@example.com");
        var service = Service.Create(storeId, "Haircut", 30);
        var professional = Professional.Create(storeId, "Jane Doe", email: "jane@example.com");
        var idField = typeof(Professional).GetProperty("Id");
        idField?.SetValue(professional, professionalId);

        var appointment = Appointment.Create(
            storeId,
            professionalId,
            serviceId,
            "John Doe",
            "+5511988888888",
            "john@example.com",
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddMinutes(30)
        );

        _resendMock
            .Setup(r => r.EmailSendAsync(It.IsAny<EmailMessage>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ResendResponse<Guid>(Guid.NewGuid(), null!));

        await _service.SendCancellationNotificationToProfessionalAsync(appointment, store, service, professional);

        _resendMock.Verify(r => r.EmailSendAsync(It.Is<EmailMessage>(m =>
            m.From.Email == "onboarding@resend.dev" &&
            m.Subject.Contains("Agendamento Cancelado") &&
            m.To.Any(t => t.Email == "jane@example.com") &&
            m.HtmlBody.Contains("John Doe")
        ), It.IsAny<CancellationToken>()), Times.Once);
    }
}
