using Moq;
using Xunit;
using HoraMarcada.Application.UseCases.Professionals;
using HoraMarcada.Application.Repositories;
using HoraMarcada.Domain.Entities;

namespace HoraMarcada.UnitTests.UseCases.Professionals;

public class ListProfessionalsUseCaseTests
{
    private readonly Mock<IProfessionalRepository> _professionalRepositoryMock;
    private readonly ListProfessionalsUseCase _useCase;

    public ListProfessionalsUseCaseTests()
    {
        _professionalRepositoryMock = new Mock<IProfessionalRepository>();
        _useCase = new ListProfessionalsUseCase(_professionalRepositoryMock.Object);
    }

    [Fact]
    public async Task ExecuteAsync_WhenActiveOnlyIsFalse_ShouldReturnAllProfessionals()
    {
        // Arrange
        var storeId = Guid.NewGuid();
        var prof1 = Professional.Create(storeId, "Jane Doe", active: true);
        var prof2 = Professional.Create(storeId, "John Smith", active: false);
        var list = new List<Professional> { prof1, prof2 }.AsReadOnly();

        _professionalRepositoryMock
            .Setup(r => r.ListByStoreIdAsync(storeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);

        // Act
        var result = await _useCase.ExecuteAsync(storeId, activeOnly: false);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, r => r.Id == prof1.Id);
        Assert.Contains(result, r => r.Id == prof2.Id);

        _professionalRepositoryMock.Verify(r => r.ListByStoreIdAsync(storeId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ExecuteAsync_WhenActiveOnlyIsTrue_ShouldReturnOnlyActiveProfessionals()
    {
        // Arrange
        var storeId = Guid.NewGuid();
        var prof1 = Professional.Create(storeId, "Jane Doe", active: true);
        var prof2 = Professional.Create(storeId, "John Smith", active: false);
        var list = new List<Professional> { prof1, prof2 }.AsReadOnly();

        _professionalRepositoryMock
            .Setup(r => r.ListByStoreIdAsync(storeId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(list);

        // Act
        var result = await _useCase.ExecuteAsync(storeId, activeOnly: true);

        // Assert
        Assert.NotNull(result);
        var singleProf = Assert.Single(result);
        Assert.Equal(prof1.Id, singleProf.Id);

        _professionalRepositoryMock.Verify(r => r.ListByStoreIdAsync(storeId, It.IsAny<CancellationToken>()), Times.Once);
    }
}
