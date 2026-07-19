using Moq;
using Xunit;
using HoraMarcada.Application.UseCases.Categories;
using HoraMarcada.Application.Repositories;
using HoraMarcada.Application.Policies;
using HoraMarcada.Domain.Entities;

namespace HoraMarcada.UnitTests.UseCases.Categories;

public class ListCategoriesUseCaseTests
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IStoreOwnershipPolicy> _ownershipPolicyMock;
    private readonly ListCategoriesUseCase _useCase;

    public ListCategoriesUseCaseTests()
    {
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _ownershipPolicyMock = new Mock<IStoreOwnershipPolicy>();
        _useCase = new ListCategoriesUseCase(
            _categoryRepositoryMock.Object,
            _ownershipPolicyMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WhenOwnershipPolicyFails_ShouldPropagateException()
    {
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();

        _ownershipPolicyMock
            .Setup(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("User does not own store."));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _useCase.ExecuteAsync(userId, storeId));
        _categoryRepositoryMock.Verify(r => r.ListByStoreIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_ShouldReturnStoreCategoriesOrderedByName()
    {
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var categories = new List<Category>
        {
            Category.Create(storeId, "Química"),
            Category.Create(storeId, "Barba"),
            Category.Create(storeId, "Cortes")
        };

        _ownershipPolicyMock
            .Setup(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _categoryRepositoryMock
            .Setup(r => r.ListByStoreIdAsync(storeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(categories);

        var result = await _useCase.ExecuteAsync(userId, storeId);

        Assert.Equal(3, result.Count);
        Assert.Equal("Barba", result[0].Name);
        Assert.Equal("Cortes", result[1].Name);
        Assert.Equal("Química", result[2].Name);
    }
}
