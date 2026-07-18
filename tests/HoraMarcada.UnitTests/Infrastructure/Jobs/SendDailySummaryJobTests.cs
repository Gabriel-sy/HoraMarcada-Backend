using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using HoraMarcada.Application.Common.Interfaces;
using HoraMarcada.Application.Repositories;
using HoraMarcada.Domain.Entities;
using HoraMarcada.Domain.Enums;
using HoraMarcada.Infrastructure.Jobs;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Xunit;

namespace HoraMarcada.UnitTests.Infrastructure.Jobs;

public class SendDailySummaryJobTests
{
    private readonly Mock<IProfessionalRepository> _professionalRepositoryMock;
    private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock;
    private readonly Mock<IServiceRepository> _serviceRepositoryMock;
    private readonly Mock<IStoreRepository> _storeRepositoryMock;
    private readonly Mock<INotificationService> _notificationServiceMock;
    private readonly SendDailySummaryJob _job;

    public SendDailySummaryJobTests()
    {
        _professionalRepositoryMock = new Mock<IProfessionalRepository>();
        _appointmentRepositoryMock = new Mock<IAppointmentRepository>();
        _serviceRepositoryMock = new Mock<IServiceRepository>();
        _storeRepositoryMock = new Mock<IStoreRepository>();
        _notificationServiceMock = new Mock<INotificationService>();

        _job = new SendDailySummaryJob(
            _professionalRepositoryMock.Object,
            _appointmentRepositoryMock.Object,
            _serviceRepositoryMock.Object,
            _storeRepositoryMock.Object,
            _notificationServiceMock.Object,
            NullLogger<SendDailySummaryJob>.Instance);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldSendDailySummary_ForActiveProfessionalsWithAppointments()
    {
        var storeId = Guid.NewGuid();
        var store = Store.Create(storeId, "Store", "store", "+5511999999999", "store@example.com");
        var professional1 = Professional.Create(storeId, "Bruno", whatsApp: "+5511977776666");
        var professional2 = Professional.Create(storeId, "Ana", whatsApp: "+5511988887777");
        var service = Service.Create(storeId, "Corte", 30);

        var appointment1 = Appointment.Create(
            storeId,
            professional1.Id,
            service.Id,
            "Maria",
            "+5511988887777",
            DateTimeOffset.UtcNow.AddHours(2),
            DateTimeOffset.UtcNow.AddHours(2.5));

        var appointment2 = Appointment.Create(
            storeId,
            professional2.Id,
            service.Id,
            "Pedro",
            "+5511988887777",
            DateTimeOffset.UtcNow.AddHours(3),
            DateTimeOffset.UtcNow.AddHours(3.5));

        _professionalRepositoryMock.Setup(r => r.ListAllActiveAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Professional> { professional1, professional2 });

        _storeRepositoryMock.Setup(r => r.GetByIdAsync(storeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(store);

        _appointmentRepositoryMock.Setup(r => r.ListByProfessionalAndDateAsync(professional1.Id, It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Appointment> { appointment1 });

        _appointmentRepositoryMock.Setup(r => r.ListByProfessionalAndDateAsync(professional2.Id, It.IsAny<DateTimeOffset>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Appointment> { appointment2 });

        _serviceRepositoryMock.Setup(r => r.ListByStoreIdAsync(storeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Service> { service });

        await _job.ExecuteAsync(CancellationToken.None);

        _notificationServiceMock.Verify(s => s.SendDailySummaryToProfessionalAsync(
            professional2,
            store,
            It.Is<IReadOnlyList<Appointment>>(list => list.Contains(appointment2)),
            It.IsAny<IReadOnlyList<Service>>(),
            It.IsAny<CancellationToken>()), Times.Once);

        _notificationServiceMock.Verify(s => s.SendDailySummaryToProfessionalAsync(
            professional1,
            store,
            It.Is<IReadOnlyList<Appointment>>(list => list.Contains(appointment1)),
            It.IsAny<IReadOnlyList<Service>>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }
}
