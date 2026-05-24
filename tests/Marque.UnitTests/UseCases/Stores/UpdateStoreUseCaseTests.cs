using Moq;
using Xunit;
using Marque.Application.UseCases.Stores;
using Marque.Application.Repositories;
using Marque.Application.Policies;
using Marque.Application.Models;
using Marque.Application.Exceptions;
using Marque.Domain.Entities;
using Marque.Domain.ValueObjects;

namespace Marque.UnitTests.UseCases.Stores;

public class UpdateStoreUseCaseTests
{
    private readonly Mock<IStoreRepository> _storeRepositoryMock;
    private readonly Mock<IStoreOwnershipPolicy> _ownershipPolicyMock;
    private readonly UpdateStoreUseCase _useCase;

    public UpdateStoreUseCaseTests()
    {
        _storeRepositoryMock = new Mock<IStoreRepository>();
        _ownershipPolicyMock = new Mock<IStoreOwnershipPolicy>();
        _useCase = new UpdateStoreUseCase(_storeRepositoryMock.Object, _ownershipPolicyMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WhenOwnershipPolicyFails_ShouldPropagateException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var input = new UpdateStoreInput(
            Name: "Updated Name",
            Slug: "updated-slug",
            WhatsApp: "+5511999999999"
        );

        _ownershipPolicyMock
            .Setup(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("User does not own store."));

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _useCase.ExecuteAsync(userId, storeId, input));
        _ownershipPolicyMock.Verify(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()), Times.Once);
        _storeRepositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _storeRepositoryMock.Verify(r => r.GetBySlugAsync(It.IsAny<Slug>(), It.IsAny<CancellationToken>()), Times.Never);
        _storeRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Store>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenStoreDoesNotExist_ShouldThrowNotFoundException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var input = new UpdateStoreInput(
            Name: "Updated Name",
            Slug: "updated-slug",
            WhatsApp: "+5511999999999"
        );

        _ownershipPolicyMock
            .Setup(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _storeRepositoryMock
            .Setup(r => r.GetByIdAsync(storeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Store?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _useCase.ExecuteAsync(userId, storeId, input));
        _ownershipPolicyMock.Verify(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()), Times.Once);
        _storeRepositoryMock.Verify(r => r.GetByIdAsync(storeId, It.IsAny<CancellationToken>()), Times.Once);
        _storeRepositoryMock.Verify(r => r.GetBySlugAsync(It.IsAny<Slug>(), It.IsAny<CancellationToken>()), Times.Never);
        _storeRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Store>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSlugChangedAndAlreadyExistsForAnotherStore_ShouldThrowConflictException()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var anotherStoreId = Guid.NewGuid();

        var input = new UpdateStoreInput(
            Name: "Updated Name",
            Slug: "new-slug",
            WhatsApp: "+5511999999999"
        );

        var existingStore = Store.Create(userId, "Old Name", "old-slug", "+5511999999999");
        // Force the ID to match our target storeId for this test case
        var idField = typeof(Store).GetProperty("Id");
        idField?.SetValue(existingStore, storeId);

        var conflictingStore = Store.Create(userId, "Another Store", "new-slug", "+5511999999999");
        idField?.SetValue(conflictingStore, anotherStoreId);

        _ownershipPolicyMock
            .Setup(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _storeRepositoryMock
            .Setup(r => r.GetByIdAsync(storeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingStore);

        _storeRepositoryMock
            .Setup(r => r.GetBySlugAsync(It.Is<Slug>(s => s.Value == input.Slug), It.IsAny<CancellationToken>()))
            .ReturnsAsync(conflictingStore);

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => _useCase.ExecuteAsync(userId, storeId, input));
        _ownershipPolicyMock.Verify(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()), Times.Once);
        _storeRepositoryMock.Verify(r => r.GetByIdAsync(storeId, It.IsAny<CancellationToken>()), Times.Once);
        _storeRepositoryMock.Verify(r => r.GetBySlugAsync(It.Is<Slug>(s => s.Value == input.Slug), It.IsAny<CancellationToken>()), Times.Once);
        _storeRepositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Store>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSlugChangedAndDoesNotExist_ShouldUpdateStore()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();

        var input = new UpdateStoreInput(
            Name: "Updated Name",
            Slug: "new-slug",
            WhatsApp: "+5511999999999",
            LogoUrl: "http://logo.com",
            Address: "123 Main St",
            AddressLat: 10.0m,
            AddressLng: 20.0m
        );

        var existingStore = Store.Create(userId, "Old Name", "old-slug", "+5511999999999");
        var idField = typeof(Store).GetProperty("Id");
        idField?.SetValue(existingStore, storeId);

        _ownershipPolicyMock
            .Setup(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _storeRepositoryMock
            .Setup(r => r.GetByIdAsync(storeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingStore);

        _storeRepositoryMock
            .Setup(r => r.GetBySlugAsync(It.Is<Slug>(s => s.Value == input.Slug), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Store?)null);

        // Act
        var result = await _useCase.ExecuteAsync(userId, storeId, input);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(input.Name, result.Name);
        Assert.Equal(input.Slug, result.Slug.Value);
        Assert.Equal(input.WhatsApp, result.WhatsApp.Value);
        Assert.Equal(input.LogoUrl, result.LogoUrl);
        Assert.Equal(input.Address, result.Address);
        Assert.Equal(input.AddressLat, result.AddressLat);
        Assert.Equal(input.AddressLng, result.AddressLng);

        _ownershipPolicyMock.Verify(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()), Times.Once);
        _storeRepositoryMock.Verify(r => r.GetByIdAsync(storeId, It.IsAny<CancellationToken>()), Times.Once);
        _storeRepositoryMock.Verify(r => r.GetBySlugAsync(It.Is<Slug>(s => s.Value == input.Slug), It.IsAny<CancellationToken>()), Times.Once);
        _storeRepositoryMock.Verify(r => r.UpdateAsync(existingStore, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSlugNotChanged_ShouldUpdateStoreWithoutCheckingSlugUniqueness()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();

        var input = new UpdateStoreInput(
            Name: "Updated Name",
            Slug: "same-slug",
            WhatsApp: "+5511999999999"
        );

        var existingStore = Store.Create(userId, "Old Name", "same-slug", "+5511888888888");
        var idField = typeof(Store).GetProperty("Id");
        idField?.SetValue(existingStore, storeId);

        _ownershipPolicyMock
            .Setup(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _storeRepositoryMock
            .Setup(r => r.GetByIdAsync(storeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingStore);

        // Act
        var result = await _useCase.ExecuteAsync(userId, storeId, input);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(input.Name, result.Name);
        Assert.Equal(input.Slug, result.Slug.Value);
        Assert.Equal(input.WhatsApp, result.WhatsApp.Value);

        _ownershipPolicyMock.Verify(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()), Times.Once);
        _storeRepositoryMock.Verify(r => r.GetByIdAsync(storeId, It.IsAny<CancellationToken>()), Times.Once);
        _storeRepositoryMock.Verify(r => r.GetBySlugAsync(It.IsAny<Slug>(), It.IsAny<CancellationToken>()), Times.Never);
        _storeRepositoryMock.Verify(r => r.UpdateAsync(existingStore, It.IsAny<CancellationToken>()), Times.Once);
    }
}
