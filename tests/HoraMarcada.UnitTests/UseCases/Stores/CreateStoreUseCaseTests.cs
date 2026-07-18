using Moq;
using Xunit;
using HoraMarcada.Application.UseCases.Stores;
using HoraMarcada.Application.Repositories;
using HoraMarcada.Application.Models;
using HoraMarcada.Application.Exceptions;
using HoraMarcada.Domain.Entities;
using HoraMarcada.Domain.ValueObjects;

namespace HoraMarcada.UnitTests.UseCases.Stores;

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
        var userId = Guid.NewGuid();
        var input = new CreateStoreInput(
            Name: "Test Store",
            Slug: "test-store",
            WhatsApp: "+5511999999999",
            Email: "store@example.com"
        );

        var existingStore = Store.Create(
            userId,
            input.Name,
            input.Slug,
            input.WhatsApp,
            input.Email
        );

        _storeRepositoryMock
            .Setup(r => r.GetBySlugAsync(It.Is<Slug>(s => s.Value == input.Slug), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingStore);

        // Act & Assert
        await Assert.ThrowsAsync<ConflictException>(() => _useCase.ExecuteAsync(userId, input));
        _storeRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Store>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenSlugDoesNotExist_ShouldCreateAndReturnStore()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var input = new CreateStoreInput(
            Name: "Test Store",
            Slug: "test-store",
            WhatsApp: "+5511999999999",
            Email: "store@example.com"
        );

        _storeRepositoryMock
            .Setup(r => r.GetBySlugAsync(It.Is<Slug>(s => s.Value == input.Slug), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Store?)null);

        // Act
        var result = await _useCase.ExecuteAsync(userId, input);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(userId, result.UserId);
        Assert.Equal(input.Name, result.Name);
        Assert.Equal(input.Slug, result.Slug);
        Assert.Equal(input.WhatsApp, result.WhatsApp);
        Assert.Equal(input.Email, result.Email);

        _storeRepositoryMock.Verify(
            r => r.AddAsync(It.Is<Store>(s => 
                s.Id == result.Id &&
                s.UserId == userId &&
                s.Name == input.Name &&
                s.Slug.Value == input.Slug &&
                s.WhatsApp.Value == input.WhatsApp &&
                s.Email.Value == input.Email &&
                s.LogoUrl == input.LogoUrl &&
                s.Address == input.Address &&
                s.AddressLat == input.AddressLat &&
                s.AddressLng == input.AddressLng
            ), It.IsAny<CancellationToken>()),
            Times.Once);
    }
}
