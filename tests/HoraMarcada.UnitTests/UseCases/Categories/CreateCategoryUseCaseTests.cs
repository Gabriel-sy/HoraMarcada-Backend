using Moq;
using Xunit;
using HoraMarcada.Application.UseCases.Categories;
using HoraMarcada.Application.Repositories;
using HoraMarcada.Application.Policies;
using HoraMarcada.Application.Models;
using HoraMarcada.Application.Exceptions;
using HoraMarcada.Domain.Entities;
using HoraMarcada.Domain.Exceptions;

namespace HoraMarcada.UnitTests.UseCases.Categories;

public class CreateCategoryUseCaseTests
{
    private readonly Mock<ICategoryRepository> _categoryRepositoryMock;
    private readonly Mock<IStoreOwnershipPolicy> _ownershipPolicyMock;
    private readonly CreateCategoryUseCase _useCase;

    public CreateCategoryUseCaseTests()
    {
        _categoryRepositoryMock = new Mock<ICategoryRepository>();
        _ownershipPolicyMock = new Mock<IStoreOwnershipPolicy>();
        _useCase = new CreateCategoryUseCase(
            _categoryRepositoryMock.Object,
            _ownershipPolicyMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WhenOwnershipPolicyFails_ShouldPropagateException()
    {
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var input = new CreateCategoryInput("Cortes");

        _ownershipPolicyMock
            .Setup(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()))
            .ThrowsAsync(new UnauthorizedAccessException("User does not own store."));

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _useCase.ExecuteAsync(userId, storeId, input));
        _categoryRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNameIsEmpty_ShouldThrowDomainValidationException()
    {
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var input = new CreateCategoryInput("   ");

        _ownershipPolicyMock
            .Setup(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await Assert.ThrowsAsync<DomainValidationException>(() => _useCase.ExecuteAsync(userId, storeId, input));
        _categoryRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenNameExceedsMaxLength_ShouldThrowDomainValidationException()
    {
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var input = new CreateCategoryInput(new string('a', Category.MaxNameLength + 1));

        _ownershipPolicyMock
            .Setup(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        await Assert.ThrowsAsync<DomainValidationException>(() => _useCase.ExecuteAsync(userId, storeId, input));
        _categoryRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenDuplicateNameExists_ShouldThrowConflictException()
    {
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var input = new CreateCategoryInput("Cortes");
        var existing = Category.Create(storeId, "cortes");

        _ownershipPolicyMock
            .Setup(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _categoryRepositoryMock
            .Setup(r => r.GetByNameAsync(storeId, input.Name, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existing);

        await Assert.ThrowsAsync<ConflictException>(() => _useCase.ExecuteAsync(userId, storeId, input));
        _categoryRepositoryMock.Verify(r => r.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ExecuteAsync_WhenValidInput_ShouldTrimNameAndReturnCategory()
    {
        var userId = Guid.NewGuid();
        var storeId = Guid.NewGuid();
        var input = new CreateCategoryInput("  Cortes  ");

        _ownershipPolicyMock
            .Setup(p => p.EnsureUserOwnsStoreAsync(userId, storeId, It.IsAny<CancellationToken>()))
            .Returns(Task.CompletedTask);

        _categoryRepositoryMock
            .Setup(r => r.GetByNameAsync(storeId, input.Name, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Category?)null);

        var result = await _useCase.ExecuteAsync(userId, storeId, input);

        Assert.Equal("Cortes", result.Name);
        Assert.Equal(storeId, result.StoreId);
        _categoryRepositoryMock.Verify(r => r.AddAsync(It.Is<Category>(c =>
            c.Name == "Cortes" &&
            c.StoreId == storeId
        ), It.IsAny<CancellationToken>()), Times.Once);
    }
}
