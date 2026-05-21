using Moq;
using Xunit;
using Marque.Application.UseCases.Appointments;
using Marque.Application.Repositories;
using Marque.Application.Policies;
using Marque.Domain.Entities;

namespace Marque.UnitTests.UseCases.Appointments;

public class ListAppointmentsUseCaseTests
{
    private readonly Mock<IAppointmentRepository> _appointmentRepositoryMock;
    private readonly Mock<IStoreOwnershipPolicy> _ownershipPolicyMock;
    private readonly ListAppointmentsUseCase _useCase;

    public ListAppointmentsUseCaseTests()
    {
        _appointmentRepositoryMock = new Mock<IAppointmentRepository>();
        _ownershipPolicyMock = new Mock<IStoreOwnershipPolicy>();
        _useCase = new ListAppointmentsUseCase(
            _appointmentRepositoryMock.Object,
            _ownershipPolicyMock.Object
        );
    }

    [Fact]
    public async Task ExecuteAsync_WhenOwnershipPolicyFails_ShouldPropagateException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var start = DateTimeOffset.UtcNow;
        var end = DateTimeOffset.UtcNow.AddDays(1);

        _ownershipPolicyMock
            .Setup(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("User does not own store."));

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _useCase.ExecuteAsync(userId, storeId, start, end));
        
        _ownershipPolicyMock.Verify(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()), Times.Once);
        _appointmentRepositoryMock.Verify(r => r.ListByStoreAsync(It.IsAny<Guid>(), It.IsAny<DateTimeOffset>(), It.IsAny<DateTimeOffset>(), It.IsAny<Guid?>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenValidInput_ShouldReturnSortedAppointments()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var start = DateTimeOffset.UtcNow;
        var end = DateTimeOffset.UtcNow.AddDays(1);
        var professionalId = Guid.NewGuid();

        _ownershipPolicyMock
            .Setup(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var app1 = Appointment.Create(
            storeId,
            professionalId,
            Guid.NewGuid(),
            "Customer 1",
            "+5511999999999",
            start.AddHours(2),
            start.AddHours(3)
        );

        var app2 = Appointment.Create(
            storeId,
            professionalId,
            Guid.NewGuid(),
            "Customer 2",
            "+5511999999999",
            start.AddHours(1),
            start.AddHours(2)
        );

        var list = new List<Appointment> { app1, app2 }.AsReadOnly();

        _appointmentRepositoryMock
            .Setup(r => r.ListByStoreAsync(storeId, start, end, professionalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);

        // Act
        var result = await _useCase.ExecuteAsync(userId, storeId, start, end, professionalId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        // Should be sorted by StartTime (app2 has earlier start time than app1)
        Assert.Equal(app2, result[0]);
        Assert.Equal(app1, result[1]);

        _ownershipPolicyMock.Verify(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()), Times.Once);
        _appointmentRepositoryMock.Verify(r => r.ListByStoreAsync(storeId, start, end, professionalId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
