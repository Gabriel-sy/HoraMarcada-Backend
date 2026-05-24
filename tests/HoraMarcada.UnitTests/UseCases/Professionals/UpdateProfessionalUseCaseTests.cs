using Moq;
using Xunit;
using HoraMarcada.Application.UseCases.Professionals;
using HoraMarcada.Application.Repositories;
using HoraMarcada.Application.Policies;
using HoraMarcada.Application.Models;
using HoraMarcada.Application.Exceptions;
using HoraMarcada.Domain.Entities;

namespace HoraMarcada.UnitTests.UseCases.Professionals;

public class UpdateProfessionalUseCaseTests
{
    private readonly Mock<IProfessionalRepository> _professionalRepositoryMock;
    private readonly Mock<IStoreOwnershipPolicy> _ownershipPolicyMock;
    private readonly UpdateProfessionalUseCase _useCase;

    public UpdateProfessionalUseCaseTests()
    {
        _professionalRepositoryMock = new Mock<IProfessionalRepository>();
        _ownershipPolicyMock = new Mock<IStoreOwnershipPolicy>();
        _useCase = new UpdateProfessionalUseCase(
            _professionalRepositoryMock.Object,
            _ownershipPolicyMock.Object
        );
    }

    [Fact]
    public async Task ExecuteAsync_WhenOwnershipPolicyFails_ShouldPropagateException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var professionalId = Guid.NewGuid();
        var input = new UpdateProfessionalInput(
            Name: "Jane Doe"
        );

        _ownershipPolicyMock
            .Setup(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("User does not own store."));

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _useCase.ExecuteAsync(userId, storeId, professionalId, input));
        _ownershipPolicyMock.Verify(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()), Times.Once);
        _professionalRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _professionalRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Professional>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenProfessionalDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var professionalId = Guid.NewGuid();
        var input = new UpdateProfessionalInput(
            Name: "Jane Doe"
        );

        _ownershipPolicyMock
            .Setup(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _professionalRepositoryMock
            .Setup(r => r.GetByIdAsync(professionalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Professional?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _useCase.ExecuteAsync(userId, storeId, professionalId, input));
        _ownershipPolicyMock.Verify(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()), Times.Once);
        _professionalRepositoryMock.Verify(r => r.GetByIdAsync(professionalId, It.IsAny<CancellationToken>()), Times.Once);
        _professionalRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Professional>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenProfessionalDoesNotBelongToStore_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var otherStoreId = Guid.NewGuid();
        var professionalId = Guid.NewGuid();

        var input = new UpdateProfessionalInput(
            Name: "Jane Doe"
        );

        _ownershipPolicyMock
            .Setup(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var professional = Professional.Create(otherStoreId, "Jane Doe");
        _professionalRepositoryMock
            .Setup(r => r.GetByIdAsync(professionalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(professional);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _useCase.ExecuteAsync(userId, storeId, professionalId, input));
        _ownershipPolicyMock.Verify(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()), Times.Once);
        _professionalRepositoryMock.Verify(r => r.GetByIdAsync(professionalId, It.IsAny<CancellationToken>()), Times.Once);
        _professionalRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Professional>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenValidInput_ShouldUpdateProfessionalAndReplaceWeeklyHours()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var professionalId = Guid.NewGuid();
        var weeklyHours = new List<WeeklyHoursInput>
        {
            new WeeklyHoursInput(
                DayOfWeek: 2,
                StartTime: new TimeOnly(10, 0),
                EndTime: new TimeOnly(17, 0),
                LunchStart: new TimeOnly(13, 0),
                LunchEnd: new TimeOnly(14, 0),
                IntervalMinutes: 45
            )
        };

        var input = new UpdateProfessionalInput(
            Name: "Jane Smith",
            PhotoUrl: "http://photo.com/janesmith",
            WhatsApp: "+5511888888888",
            Email: "jane.smith@example.com",
            WeeklyHours: weeklyHours
        );

        _ownershipPolicyMock
            .Setup(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var professional = Professional.Create(storeId, "Jane Doe");
        var idField = typeof(Professional).GetProperty("Id");
        idField?.SetValue(professional, professionalId);
        professional.AddWeeklyHours(1, new TimeOnly(9, 0), new TimeOnly(18, 0)); // Old hours

        _professionalRepositoryMock
            .Setup(r => r.GetByIdAsync(professionalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(professional);

        // Act
        var result = await _useCase.ExecuteAsync(userId, storeId, professionalId, input);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(input.Name, result.Name);
        Assert.Equal(input.PhotoUrl, result.PhotoUrl);
        Assert.Equal(input.WhatsApp, result.WhatsApp?.Value);
        Assert.Equal(input.Email, result.Email?.Value);
        
        var singleSchedule = Assert.Single(result.WeeklySchedules);
        Assert.Equal(2, singleSchedule.DayOfWeek);
        Assert.Equal(new TimeOnly(10, 0), singleSchedule.StartTime);
        Assert.Equal(new TimeOnly(17, 0), singleSchedule.EndTime);
        Assert.Equal(new TimeOnly(13, 0), singleSchedule.LunchStart);
        Assert.Equal(new TimeOnly(14, 0), singleSchedule.LunchEnd);
        Assert.Equal(45, singleSchedule.IntervalMinutes);

        _ownershipPolicyMock.Verify(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()), Times.Once);
        _professionalRepositoryMock.Verify(r => r.GetByIdAsync(professionalId, It.IsAny<CancellationToken>()), Times.Once);
        _professionalRepositoryMock.Verify(r => r.UpdateAsync(professional, It.IsAny<CancellationToken>()), Times.Once);
    }
}
