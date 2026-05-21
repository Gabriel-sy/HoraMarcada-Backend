using Moq;
using Xunit;
using Marque.Application.UseCases.Professionals;
using Marque.Application.Repositories;
using Marque.Application.Policies;
using Marque.Application.Exceptions;
using Marque.Domain.Entities;

namespace Marque.UnitTests.UseCases.Professionals;

public class DeactivateProfessionalUseCaseTests
{
    private readonly Mock<IProfessionalRepository> _professionalRepositoryMock;
    private readonly Mock<IStoreOwnershipPolicy> _ownershipPolicyMock;
    private readonly DeactivateProfessionalUseCase _useCase;

    public DeactivateProfessionalUseCaseTests()
    {
        _professionalRepositoryMock = new Mock<IProfessionalRepository>();
        _ownershipPolicyMock = new Mock<IStoreOwnershipPolicy>();
        _useCase = new DeactivateProfessionalUseCase(
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

        _ownershipPolicyMock
            .Setup(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("User does not own store."));

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _useCase.ExecuteAsync(userId, storeId, professionalId));
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

        _ownershipPolicyMock
            .Setup(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _professionalRepositoryMock
            .Setup(r => r.GetByIdAsync(professionalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Professional?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _useCase.ExecuteAsync(userId, storeId, professionalId));
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

        _ownershipPolicyMock
            .Setup(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var professional = Professional.Create(otherStoreId, "Jane Doe");
        _professionalRepositoryMock
            .Setup(r => r.GetByIdAsync(professionalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(professional);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _useCase.ExecuteAsync(userId, storeId, professionalId));
        _ownershipPolicyMock.Verify(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()), Times.Once);
        _professionalRepositoryMock.Verify(r => r.GetByIdAsync(professionalId, It.IsAny<CancellationToken>()), Times.Once);
        _professionalRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Professional>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenValidInput_ShouldDeactivateProfessional()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var professionalId = Guid.NewGuid();

        _ownershipPolicyMock
            .Setup(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        var professional = Professional.Create(storeId, "Jane Doe", active: true);
        var idField = typeof(Professional).GetProperty("Id");
        idField?.SetValue(professional, professionalId);

        _professionalRepositoryMock
            .Setup(r => r.GetByIdAsync(professionalId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(professional);

        // Act
        await _useCase.ExecuteAsync(userId, storeId, professionalId);

        // Assert
        Assert.False(professional.Active);
        _ownershipPolicyMock.Verify(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()), Times.Once);
        _professionalRepositoryMock.Verify(r => r.GetByIdAsync(professionalId, It.IsAny<CancellationToken>()), Times.Once);
        _professionalRepositoryMock.Verify(r => r.UpdateAsync(professional, It.IsAny<CancellationToken>()), Times.Once);
    }
}
