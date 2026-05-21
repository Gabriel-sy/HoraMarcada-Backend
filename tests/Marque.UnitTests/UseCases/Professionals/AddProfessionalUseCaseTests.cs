using Moq;
using Xunit;
using System.Linq;
using Marque.Application.UseCases.Professionals;
using Marque.Application.Repositories;
using Marque.Application.Policies;
using Marque.Application.Models;
using Marque.Domain.Entities;

namespace Marque.UnitTests.UseCases.Professionals;

public class AddProfessionalUseCaseTests
{
    private readonly Mock<IProfessionalRepository> _professionalRepositoryMock;
    private readonly Mock<IStoreOwnershipPolicy> _ownershipPolicyMock;
    private readonly AddProfessionalUseCase _useCase;

    public AddProfessionalUseCaseTests()
    {
        _professionalRepositoryMock = new Mock<IProfessionalRepository>();
        _ownershipPolicyMock = new Mock<IStoreOwnershipPolicy>();
        _useCase = new AddProfessionalUseCase(
            _professionalRepositoryMock.Object,
            _ownershipPolicyMock.Object
        );
    }

    [Fact]
    public async Task ExecuteAsync_WhenOwnershipPolicyFails_ShouldPropagateException()
    {
        // Arrange
        var input = new AddProfessionalInput(
            UserId: Guid.NewGuid(),
            StoreId: Guid.NewGuid(),
            Name: "Jane Doe"
        );

        _ownershipPolicyMock
            .Setup(p => p.EnsureUserOwnsStoreAsync(input.UserId, input.StoreId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("User does not own store."));

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _useCase.ExecuteAsync(input));
        _ownershipPolicyMock.Verify(p => p.EnsureUserOwnsStoreAsync(input.UserId, input.StoreId, It.IsAny<CancellationToken>()), Times.Once);
        _professionalRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Professional>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenValidInputWithoutWeeklyHours_ShouldCreateProfessional()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();

        var input = new AddProfessionalInput(
            UserId: userId,
            StoreId: storeId,
            Name: "Jane Doe",
            PhotoUrl: "http://photo.com/jane",
            WhatsApp: "+5511999999999"
        );

        _ownershipPolicyMock
            .Setup(p => p.EnsureUserOwnsStoreAsync(input.UserId, input.StoreId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _useCase.ExecuteAsync(input);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(input.Name, result.Name);
        Assert.Equal(input.PhotoUrl, result.PhotoUrl);
        Assert.Equal(input.WhatsApp, result.WhatsApp?.Value);
        Assert.Empty(result.WeeklySchedules);

        _ownershipPolicyMock.Verify(p => p.EnsureUserOwnsStoreAsync(input.UserId, input.StoreId, It.IsAny<CancellationToken>()), Times.Once);
        _professionalRepositoryMock.Verify(
            r => r.AddAsync(It.Is<Professional>(p => 
                p.Id == result.Id &&
                p.StoreId == input.StoreId &&
                p.Name == input.Name &&
                p.PhotoUrl == input.PhotoUrl &&
                p.WhatsApp.Value == input.WhatsApp
            ), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }

    [Fact]
    public async Task ExecuteAsync_WhenValidInputWithWeeklyHours_ShouldCreateProfessionalWithWeeklyHours()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var weeklyHours = new List<WeeklyHoursInput>
        {
            new WeeklyHoursInput(
                DayOfWeek: 1,
                StartTime: new TimeOnly(9, 0),
                EndTime: new TimeOnly(18, 0),
                LunchStart: new TimeOnly(12, 0),
                LunchEnd: new TimeOnly(13, 0),
                IntervalMinutes: 30
            )
        };

        var input = new AddProfessionalInput(
            UserId: userId,
            StoreId: storeId,
            Name: "Jane Doe",
            WeeklyHours: weeklyHours
        );

        _ownershipPolicyMock
            .Setup(p => p.EnsureUserOwnsStoreAsync(input.UserId, input.StoreId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _useCase.ExecuteAsync(input);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(input.Name, result.Name);
        var singleSchedule = Assert.Single(result.WeeklySchedules);
        Assert.Equal(1, singleSchedule.DayOfWeek);
        Assert.Equal(new TimeOnly(9, 0), singleSchedule.StartTime);
        Assert.Equal(new TimeOnly(18, 0), singleSchedule.EndTime);
        Assert.Equal(new TimeOnly(12, 0), singleSchedule.LunchStart);
        Assert.Equal(new TimeOnly(13, 0), singleSchedule.LunchEnd);
        Assert.Equal(30, singleSchedule.IntervalMinutes);

        _ownershipPolicyMock.Verify(p => p.EnsureUserOwnsStoreAsync(input.UserId, input.StoreId, It.IsAny<CancellationToken>()), Times.Once);
        _professionalRepositoryMock.Verify(
            r => r.AddAsync(It.Is<Professional>(p => 
                p.Id == result.Id &&
                p.StoreId == input.StoreId &&
                p.Name == input.Name &&
                p.WeeklySchedules.Count == 1 &&
                p.WeeklySchedules.First().DayOfWeek == 1 &&
                p.WeeklySchedules.First().StartTime == new TimeOnly(9, 0) &&
                p.WeeklySchedules.First().EndTime == new TimeOnly(18, 0) &&
                p.WeeklySchedules.First().LunchStart == new TimeOnly(12, 0) &&
                p.WeeklySchedules.First().LunchEnd == new TimeOnly(13, 0) &&
                p.WeeklySchedules.First().IntervalMinutes == 30
            ), It.IsAny<CancellationToken>()),
            Times.Once
        );
    }
}
