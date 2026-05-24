using HoraMarcada.Domain.Exceptions;
using HoraMarcada.Domain.ValueObjects;
using Xunit;

namespace HoraMarcada.UnitTests.Domain.ValueObjects;

public class EmailTests
{
    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name+tag@domain.co.uk")]
    [InlineData("first.last@sub.domain.org")]
    public void Create_WithValidEmail_ShouldReturnEmail(string value)
    {
        var result = Email.Create(value);

        Assert.NotNull(result);
        Assert.Equal(value.ToLowerInvariant(), result.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Create_WithEmptyOrNullEmail_ShouldThrowDomainValidationException(string? value)
    {
#pragma warning disable CS8604
        Assert.Throws<DomainValidationException>(() => Email.Create(value!));
#pragma warning restore CS8604
    }

    [Theory]
    [InlineData("invalid-email")]
    [InlineData("@domain.com")]
    [InlineData("user@")]
    [InlineData("user@domain")]
    public void Create_WithInvalidEmailFormat_ShouldThrowDomainValidationException(string value)
    {
        Assert.Throws<DomainValidationException>(() => Email.Create(value));
    }

    [Fact]
    public void CreateOptional_WithNullOrWhitespace_ShouldReturnNull()
    {
        var resultNull = Email.CreateOptional(null);
        var resultEmpty = Email.CreateOptional("");
        var resultWhitespace = Email.CreateOptional("   ");

        Assert.Null(resultNull);
        Assert.Null(resultEmpty);
        Assert.Null(resultWhitespace);
    }

    [Fact]
    public void CreateOptional_WithValidEmail_ShouldReturnEmail()
    {
        var result = Email.CreateOptional("test@example.com");

        Assert.NotNull(result);
        Assert.Equal("test@example.com", result.Value);
    }

    [Fact]
    public void ToString_ShouldReturnEmailValue()
    {
        var email = Email.Create("TEST@example.com");

        Assert.Equal("test@example.com", email.ToString());
    }
}
