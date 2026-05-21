using Moq;
using Xunit;
using Marque.Application.UseCases.Stores;
using Marque.Application.Repositories;
using Marque.Application.Models;
using Marque.Application.Exceptions;
using Marque.Domain.Entities;
using Marque.Domain.ValueObjects;

namespace Marque.UnitTests.UseCases.Stores;

public class CreateStoreUseCaseTests
{
    private readonly Mock<IStoreRepository> _storeRepositoryMock;
    private readonly CreateStoreUseCase _useCase;

    public CreateStoreUseCaseTests()
    {
        _storeRepositoryMock = new Mock<IStoreRepository>();
        _useCase = new CreateStoreUseCase(_storeRepositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSlugAlreadyExists_ShouldThrowConflictException()
    {
        // Arrange
        var input = new CreateStoreInput(
            UserId: Guid.NewGuid(),
            Name: "Test Store",
            Slug: "test-store",
            WhatsApp: "+5511999999999"
        );

        var existingStore = Store.Create(
            input.UserId,
            input.Name,
            input.Slug,
            input.WhatsApp
        );

        _storeRepositoryMock
            .Setup(r => r.GetBySlugAsync(It.Is<Slug>(s => s.Value == input.Slug), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingStore);

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => _useCase.ExecuteAsync(input));
        _storeRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Store>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSlugDoesNotExist_ShouldCreateAndReturnStore()
    {
        // Arrange
        var input = new CreateStoreInput(
            UserId: Guid.NewGuid(),
            Name: "Test Store",
            Slug: "test-store",
            WhatsApp: "+5511999999999"
        );

        _storeRepositoryMock
            .Setup(r => r.GetBySlugAsync(It.Is<Slug>(s => s.Value == input.Slug), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Store?)null);

        // Act
        var result = await _useCase.ExecuteAsync(input);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(input.UserId, result.UserId);
        Assert.Equal(input.Name, result.Name);
        Assert.Equal(input.Slug, result.Slug.Value);
        Assert.Equal(input.WhatsApp, result.WhatsApp.Value);

        _storeRepositoryMock.Verify(
            r => r.AddAsync(It.Is<Store>(s => 
                s.Id == result.Id &&
                s.UserId == input.UserId &&
                s.Name == input.Name &&
                s.Slug.Value == input.Slug &&
                s.WhatsApp.Value == input.WhatsApp &&
                s.LogoUrl == input.LogoUrl &&
                s.Address == input.Address &&
                s.AddressLat == input.AddressLat &&
                s.AddressLng == input.AddressLng
            ), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
