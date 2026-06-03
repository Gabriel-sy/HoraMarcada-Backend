using Xunit;
using HoraMarcada.Infrastructure.Persistence.Extensions;

namespace HoraMarcada.UnitTests.Infrastructure.Extensions;

public class StringExtensionsTests
{
    [Theory]
    [InlineData("João", "Joao")]
    [InlineData("São Paulo", "Sao Paulo")]
    [InlineData("ÁÉÍÓÚáéíóúâêîôûàèìòùäëïöüãõçñ", "AEIOUaeiouaeiouaeiouaeiouaocn")]
    [InlineData("plain text", "plain text")]
    [InlineData("", "")]
    [InlineData(null, null)]
    public void RemoveDiacritics_ShouldNormalizeString(string? input, string? expected)
    {
        var result = input?.RemoveDiacritics();
        Assert.Equal(expected, result);
    }
}
