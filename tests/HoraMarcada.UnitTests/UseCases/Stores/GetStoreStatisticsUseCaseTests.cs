using Moq;
using Xunit;
using HoraMarcada.Application.UseCases.Stores;
using HoraMarcada.Application.Repositories;
using HoraMarcada.Application.Policies;
using HoraMarcada.Application.Models;
using HoraMarcada.Application.Exceptions;
using HoraMarcada.Domain.Entities;
using HoraMarcada.Domain.Enums;

namespace HoraMarcada.UnitTests.UseCases.Stores;

public class GetStoreStatisticsUseCaseTests
{
    private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock;
    private readonly Mock<IServiceRepository> _serviceRepositoryMock;
    private readonly Mock<IProfessionalRepository> _professionalRepositoryMock;
    private readonly Mock<IStoreOwnershipPolicy> _ownershipPolicyMock;
    private readonly GetStoreStatisticsUseCase _useCase;

    public GetStoreStatisticsUseCaseTests()
    {
        _appointmentRepositoryMock = new Mock<IAppointmentRepository>();
        _serviceRepositoryMock = new Mock<IServiceRepository>();
        _professionalRepositoryMock = new Mock<IProfessionalRepository>();
        _ownershipPolicyMock = new Mock<IStoreOwnershipPolicy>();

        _useCase = new GetStoreStatisticsUseCase(
            _appointmentRepositoryMock.Object,
            _serviceRepositoryMock.Object,
            _professionalRepositoryMock.Object,
            _ownershipPolicyMock.Object
        );
    }

    [Fact]
    public async Task ExecuteAsync_WhenUserDoesNotOwnStore_ShouldThrowNotFoundException()
    {
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var input = new GetStoreStatisticsInput(storeId, DateTimeOffset.UtcNow.AddDays(-30), DateTimeOffset.UtcNow);

        _ownershipPolicyMock
            .Setup(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new NotFoundException($"Store with ID {storeId} not found."));

        await Assert.ThrowsAsync<NotFoundException>(() => _useCase.ExecuteAsync(userId, input));
    }

    [Fact]
    public async Task ExecuteAsync_WhenStoreExists_ShouldReturnAggregatedStatistics()
    {
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var professionalId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();

        var localTime = new DateTimeOffset(2026, 6, 1, 12, 0, 0, TimeSpan.FromHours(-3));
        var input = new GetStoreStatisticsInput(
            storeId,
            localTime.AddDays(-10),
            localTime
        );

        _ownershipPolicyMock
            .Setup(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var service = Service.Create(storeId, "Corte de Cabelo", 30, 50.00m);
        var professional = Professional.Create(storeId, "João Barbeiro");

        var app1 = Appointment.Create(
            storeId,
            professional.Id,
            service.Id,
            "Cliente A",
            "+5511999999999",
            localTime.AddDays(-2),
            localTime.AddDays(-2).AddMinutes(30),
            status: AppointmentStatus.Completed
        );

        var app2 = Appointment.Create(
            storeId,
            professional.Id,
            service.Id,
            "Cliente B",
            "+5511999999999",
            localTime.AddDays(-1),
            localTime.AddDays(-1).AddMinutes(30),
            status: AppointmentStatus.Confirmed
        );

        var appCanceled = Appointment.Create(
            storeId,
            professional.Id,
            service.Id,
            "Cliente C",
            "+5511999999999",
            localTime.AddDays(-3),
            localTime.AddDays(-3).AddMinutes(30),
            status: AppointmentStatus.Canceled
        );

        var appointmentsList = new List<Appointment> { app1, app2, appCanceled };

        _appointmentRepositoryMock
            .Setup(r => r.ListByStoreAsync(storeId, input.StartDate, input.EndDate, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointmentsList);

        var start15DaysAgo = new DateTimeOffset(2026, 6, 1, 23, 59, 59, TimeSpan.FromHours(-3)).AddDays(-14).Date;
        var start15DaysAgoUtc = new DateTimeOffset(start15DaysAgo.Year, start15DaysAgo.Month, start15DaysAgo.Day, 0, 0, 0, TimeSpan.FromHours(-3)).ToUniversalTime();
        var endTodayUtc = new DateTimeOffset(2026, 6, 1, 23, 59, 59, TimeSpan.FromHours(-3)).ToUniversalTime();

        _appointmentRepositoryMock
            .Setup(r => r.ListByStoreAsync(storeId, start15DaysAgoUtc, endTodayUtc, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync(appointmentsList);

        _serviceRepositoryMock
            .Setup(r => r.ListByStoreIdAsync(storeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Service> { service });

        _professionalRepositoryMock
            .Setup(r => r.ListByStoreIdAsync(storeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new List<Professional> { professional });

        var result = await _useCase.ExecuteAsync(userId, input);

        Assert.NotNull(result);
        Assert.Equal(100.00m, result.EstimatedRevenue);
        Assert.Equal(2, result.TotalAppointments);

        Assert.Single(result.MostPopularServices);
        Assert.Equal("Corte de Cabelo", result.MostPopularServices[0].ServiceName);
        Assert.Equal(2, result.MostPopularServices[0].Count);
        Assert.Equal(100.00m, result.MostPopularServices[0].Revenue);

        Assert.Single(result.ProfessionalOccupation);
        Assert.Equal("João Barbeiro", result.ProfessionalOccupation[0].ProfessionalName);
        Assert.Equal(2, result.ProfessionalOccupation[0].Count);
        Assert.Equal(60, result.ProfessionalOccupation[0].TotalMinutes);

        Assert.Equal(15, result.DailyAppointments.Count);
        var dayOffsetMinus2 = result.DailyAppointments.FirstOrDefault(d => d.Date == localTime.AddDays(-2).ToString("yyyy-MM-dd"));
        Assert.NotNull(dayOffsetMinus2);
        Assert.Equal(1, dayOffsetMinus2.Count);

        var dayOffsetMinus1 = result.DailyAppointments.FirstOrDefault(d => d.Date == localTime.AddDays(-1).ToString("yyyy-MM-dd"));
        Assert.NotNull(dayOffsetMinus1);
        Assert.Equal(1, dayOffsetMinus1.Count);
    }
}
