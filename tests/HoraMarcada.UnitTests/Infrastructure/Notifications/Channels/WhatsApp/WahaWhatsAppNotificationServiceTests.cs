using HoraMarcada.Domain.Entities;
using HoraMarcada.Infrastructure.Notifications;
using HoraMarcada.Infrastructure.Notifications.Channels.WhatsApp;
using HoraMarcada.Infrastructure.Notifications.Channels.WhatsApp.Client;
using HoraMarcada.Infrastructure.Notifications.Channels.WhatsApp.Simulation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace HoraMarcada.UnitTests.Infrastructure.Notifications.Channels.WhatsApp;

public class WahaWhatsAppNotificationServiceTests
{
    private readonly Mock<IWahaClient> _wahaClientMock;
    private readonly Mock<IWhatsAppHumanBehavior> _humanBehaviorMock;
    private readonly WahaWhatsAppNotificationService _service;

    public WahaWhatsAppNotificationServiceTests()
    {
        _wahaClientMock = new Mock<IWahaClient>();
        _humanBehaviorMock = new Mock<IWhatsAppHumanBehavior>();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> { ["FrontendUrl"] = "https://horamarcada.com" })
            .Build();

        _service = new WahaWhatsAppNotificationService(
            _wahaClientMock.Object,
            _humanBehaviorMock.Object,
            configuration,
            NullLogger<WahaWhatsAppNotificationService>.Instance);
    }

    [Fact]
    public async Task SendBookingConfirmationToCustomerAsync_ShouldResolveChatIdAndSendText()
    {
        var storeId = Guid.NewGuid();
        var store = Store.Create(storeId, "Store", "store", "+5511999999999", "store@example.com");
        var service = Service.Create(storeId, Guid.NewGuid(), "Corte", 30);
        var professional = Professional.Create(storeId, "João");
        var appointment = Appointment.Create(
            storeId,
            professional.Id,
            service.Id,
            "Maria",
            "+5511988887777",
            DateTimeOffset.UtcNow.AddDays(1),
            DateTimeOffset.UtcNow.AddDays(1).AddMinutes(30),
            cancelToken: "token123");

        _wahaClientMock.Setup(c => c.GetSessionStatusAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(WahaSessionStatus.Working);
        _wahaClientMock.Setup(c => c.CheckContactExistsAsync("5511988887777", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WahaContactCheckResult(true, "5511988887777@c.us"));

        await _service.SendBookingConfirmationToCustomerAsync(appointment, store, service, professional);

        _humanBehaviorMock.Verify(h => h.WaitInterMessageDelayAsync(It.IsAny<CancellationToken>()), Times.Once);
        _humanBehaviorMock.Verify(h => h.SimulateTypingBeforeSendAsync(
            _wahaClientMock.Object,
            "5511988887777@c.us",
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Once);
        _humanBehaviorMock.Verify(h => h.NotifyMessageSent(), Times.Once);
        _humanBehaviorMock.Verify(h => h.WaitFollowUpDelayAsync(It.IsAny<CancellationToken>()), Times.Never);
        _humanBehaviorMock.Verify(h => h.SimulateShortTypingBeforeSendAsync(
            It.IsAny<IWahaClient>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
        _wahaClientMock.Verify(c => c.SendLocationAsync(
            It.IsAny<string>(),
            It.IsAny<double>(),
            It.IsAny<double>(),
            It.IsAny<CancellationToken>()), Times.Never);
        _wahaClientMock.Verify(c => c.SendTextAsync(
            "5511988887777@c.us",
            It.Is<string>(t => t.Contains("Maria") && t.Contains("token123")),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendBookingConfirmationToCustomerAsync_WhenStoreHasCoordinates_ShouldSendLocationAfterText()
    {
        var storeId = Guid.NewGuid();
        var store = Store.Create(
            storeId,
            "Barbearia",
            "barbearia",
            "+5511999999999",
            "store@example.com",
            address: "Rua das Flores, 123",
            addressLat: -23.5505m,
            addressLng: -46.6333m);
        var service = Service.Create(storeId, Guid.NewGuid(), "Corte", 30);
        var professional = Professional.Create(storeId, "João");
        var appointment = Appointment.Create(
            storeId,
            professional.Id,
            service.Id,
            "Maria",
            "+5511988887777",
            DateTimeOffset.UtcNow.AddDays(1),
            DateTimeOffset.UtcNow.AddDays(1).AddMinutes(30));

        _wahaClientMock.Setup(c => c.GetSessionStatusAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(WahaSessionStatus.Working);
        _wahaClientMock.Setup(c => c.CheckContactExistsAsync("5511988887777", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WahaContactCheckResult(true, "5511988887777@c.us"));

        await _service.SendBookingConfirmationToCustomerAsync(appointment, store, service, professional);

        _humanBehaviorMock.Verify(h => h.WaitFollowUpDelayAsync(It.IsAny<CancellationToken>()), Times.Once);
        _humanBehaviorMock.Verify(h => h.SimulateShortTypingBeforeSendAsync(
            _wahaClientMock.Object,
            "5511988887777@c.us",
            It.IsAny<CancellationToken>()), Times.Once);
        _wahaClientMock.Verify(c => c.SendLocationAsync(
            "5511988887777@c.us",
            -23.5505,
            -46.6333,
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task SendBookingConfirmationToCustomerAsync_WhenNumberNotExists_ShouldSkipSend()
    {
        var storeId = Guid.NewGuid();
        var store = Store.Create(storeId, "Store", "store", "+5511999999999", "store@example.com");
        var service = Service.Create(storeId, Guid.NewGuid(), "Corte", 30);
        var professional = Professional.Create(storeId, "João");
        var appointment = Appointment.Create(
            storeId,
            professional.Id,
            service.Id,
            "Maria",
            "+5511988887777",
            DateTimeOffset.UtcNow.AddDays(1),
            DateTimeOffset.UtcNow.AddDays(1).AddMinutes(30));

        _wahaClientMock.Setup(c => c.GetSessionStatusAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(WahaSessionStatus.Working);
        _wahaClientMock.Setup(c => c.CheckContactExistsAsync("5511988887777", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WahaContactCheckResult(false, null));

        await _service.SendBookingConfirmationToCustomerAsync(appointment, store, service, professional);

        _humanBehaviorMock.Verify(h => h.WaitInterMessageDelayAsync(It.IsAny<CancellationToken>()), Times.Never);
        _humanBehaviorMock.Verify(h => h.SimulateTypingBeforeSendAsync(
            It.IsAny<IWahaClient>(),
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
        _humanBehaviorMock.Verify(h => h.NotifyMessageSent(), Times.Never);
        _wahaClientMock.Verify(c => c.SendTextAsync(
            It.IsAny<string>(),
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task SendDailySummaryToProfessionalAsync_ShouldResolveChatIdAndSendFormattedText()
    {
        var storeId = Guid.NewGuid();
        var store = Store.Create(storeId, "Store", "store", "+5511999999999", "store@example.com");
        var service = Service.Create(storeId, Guid.NewGuid(), "Corte", 30);
        var professional = Professional.Create(storeId, "João", whatsApp: "+5511977776666");
        var appointment = Appointment.Create(
            storeId,
            professional.Id,
            service.Id,
            "Maria",
            "+5511988887777",
            DateTimeOffset.UtcNow.AddDays(1),
            DateTimeOffset.UtcNow.AddDays(1).AddMinutes(30),
            cancelToken: "token123");

        _wahaClientMock.Setup(c => c.GetSessionStatusAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(WahaSessionStatus.Working);
        _wahaClientMock.Setup(c => c.CheckContactExistsAsync("5511977776666", It.IsAny<CancellationToken>()))
            .ReturnsAsync(new WahaContactCheckResult(true, "5511977776666@c.us"));

        await _service.SendDailySummaryToProfessionalAsync(professional, store, new[] { appointment }, new[] { service });

        _humanBehaviorMock.Verify(h => h.WaitInterMessageDelayAsync(It.IsAny<CancellationToken>()), Times.Once);
        _humanBehaviorMock.Verify(h => h.SimulateTypingBeforeSendAsync(
            _wahaClientMock.Object,
            "5511977776666@c.us",
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Once);
        _wahaClientMock.Verify(c => c.SendTextAsync(
            "5511977776666@c.us",
            It.Is<string>(t => t.Contains("João") && t.Contains("Maria") && t.Contains("Corte") && t.Contains("+5511988887777") && t.Contains("token123")),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
