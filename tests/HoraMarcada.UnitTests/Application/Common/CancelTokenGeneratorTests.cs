using HoraMarcada.Application.Common;
using HoraMarcada.Application.Repositories;
using HoraMarcada.Domain.Entities;
using Moq;
using Xunit;

namespace HoraMarcada.UnitTests.Application.Common;

public class CancelTokenGeneratorTests
{
    private readonly CancelTokenGenerator _generator = new();

    [Fact]
    public void Generate_ShouldReturnSixCharacters()
    {
        var token = _generator.Generate();

        Assert.Equal(5, token.Length);
    }

    [Fact]
    public void Generate_ShouldUseBase62Charset()
    {
        const string charset = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        for (var i = 0; i < 50; i++)
        {
            var token = _generator.Generate();
            Assert.All(token, character => Assert.Contains(character, charset));
        }
    }

    [Fact]
    public async Task GenerateUniqueAsync_ShouldRetryWhenTokenExists()
    {
        var storeId = Guid.NewGuid();
        var professionalId = Guid.NewGuid();
        var serviceId = Guid.NewGuid();
        var existingAppointment = Appointment.Create(
            storeId,
            professionalId,
            serviceId,
            "John",
            "+5511988887777",
            DateTimeOffset.UtcNow,
            DateTimeOffset.UtcNow.AddMinutes(30));

        var repositoryMock = new Mock<IAppointmentRepository>();
        repositoryMock
            .SetupSequence(r => r.GetByCancelTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingAppointment)
            .ReturnsAsync((Appointment?)null);

        var token = await _generator.GenerateUniqueAsync(repositoryMock.Object);

        Assert.Equal(5, token.Length);
        repositoryMock.Verify(
            r => r.GetByCancelTokenAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()),
            Times.Exactly(2));
    }
}
