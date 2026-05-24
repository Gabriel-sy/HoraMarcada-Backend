using Moq;
using Xunit;
using HoraMarcada.Application.UseCases.Services;
using HoraMarcada.Application.Repositories;
using HoraMarcada.Application.Policies;
using HoraMarcada.Application.Models;
using HoraMarcada.Domain.Entities;
using HoraMarcada.Domain.Exceptions;

namespace HoraMarcada.UnitTests.UseCases.Services;

public class AddServiceUseCaseTests
{
    private readonly Mock<IServiceRepository> _serviceRepositoryMock;
    private readonly Mock<IProfessionalRepository> _professionalRepositoryMock;
    private readonly Mock<IStoreOwnershipPolicy> _ownershipPolicyMock;
    private readonly AddServiceUseCase _useCase;

    public AddServiceUseCaseTests()
    {
        _serviceRepositoryMock = new Mock<IServiceRepository>();
        _professionalRepositoryMock = new Mock<IProfessionalRepository>();
        _ownershipPolicyMock = new Mock<IStoreOwnershipPolicy>();
        _useCase = new AddServiceUseCase(
            _serviceRepositoryMock.Object,
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
        var input = new AddServiceInput(
            Name: "Haircut",
            DurationMinutes: 30
        );

        _ownershipPolicyMock
            .Setup(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("User does not own store."));

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _useCase.ExecuteAsync(userId, storeId, input));
        _ownershipPolicyMock.Verify(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()), Times.Once);
        _professionalRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _serviceRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Service>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenProfessionalDoesNotBelongToStore_ShouldThrowDomainValidationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var otherStoreId = Guid.NewGuid();
        var professionalId = Guid.NewGuid();

        var input = new AddServiceInput(
            Name: "Haircut",
            DurationMinutes: 30,
            ProfessionalIds: [professionalId]
        );

        _ownershipPolicyMock
            .Setup(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var professional = Professional.Create(otherStoreId, "John Doe");
        _professionalRepositoryMock
            .Setup(r => r.GetByIdAsync(professionalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(professional);

        // Act & Assert
        await Assert.ThrowsAsync<DomainValidationException>(() => _useCase.ExecuteAsync(userId, storeId, input));
        _ownershipPolicyMock.Verify(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()), Times.Once);
        _professionalRepositoryMock.Verify(r => r.GetByIdAsync(professionalId, It.IsAny<CancellationToken>()), Times.Once);
        _serviceRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Service>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenProfessionalDoesNotExist_ShouldThrowDomainValidationException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var professionalId = Guid.NewGuid();

        var input = new AddServiceInput(
            Name: "Haircut",
            DurationMinutes: 30,
            ProfessionalIds: [professionalId]
        );

        _ownershipPolicyMock
            .Setup(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _professionalRepositoryMock
            .Setup(r => r.GetByIdAsync(professionalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Professional?)null);

        // Act & Assert
        await Assert.ThrowsAsync<DomainValidationException>(() => _useCase.ExecuteAsync(userId, storeId, input));
        _ownershipPolicyMock.Verify(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()), Times.Once);
        _professionalRepositoryMock.Verify(r => r.GetByIdAsync(professionalId, It.IsAny<CancellationToken>()), Times.Once);
        _serviceRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Service>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenValidInput_ShouldCreateAndAssignProfessionals()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var professionalId = Guid.NewGuid();

        var input = new AddServiceInput(
            Name: "Haircut",
            DurationMinutes: 30,
            Price: 50.0m,
            ProfessionalIds: [professionalId]
        );

        _ownershipPolicyMock
            .Setup(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var professional = Professional.Create(storeId, "John Doe");
        var idField = typeof(Professional).GetProperty("Id");
        idField?.SetValue(professional, professionalId);

        _professionalRepositoryMock
            .Setup(r => r.GetByIdAsync(professionalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(professional);

        // Act
        var result = await _useCase.ExecuteAsync(userId, storeId, input);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(input.Name, result.Name);
        Assert.Equal(input.DurationMinutes, result.DurationMinutes);
        Assert.Equal(input.Price, result.Price?.Amount);
        Assert.Contains(professionalId, result.ProfessionalIds);

        _ownershipPolicyMock.Verify(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()), Times.Once);
        _professionalRepositoryMock.Verify(r => r.GetByIdAsync(professionalId, It.IsAny<CancellationToken>()), Times.Once);
        _serviceRepositoryMock.Verify(r => r.AddAsync(It.Is<Service>(s =>
            s.Id == result.Id &&
            s.StoreId == storeId &&
            s.Name == input.Name &&
            s.DurationMinutes == input.DurationMinutes &&
            s.Price.Amount == input.Price &&
            s.ProfessionalIds.Contains(professionalId)
        ), It.IsAny<CancellationToken>()), Times.Once);
    }
}
