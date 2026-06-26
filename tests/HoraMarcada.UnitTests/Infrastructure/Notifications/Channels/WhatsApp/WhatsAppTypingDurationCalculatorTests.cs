using HoraMarcada.Infrastructure.Notifications.Channels.WhatsApp.Simulation;
using Xunit;

namespace HoraMarcada.UnitTests.Infrastructure.Notifications.Channels.WhatsApp;

public class WhatsAppTypingDurationCalculatorTests
{
    private readonly WhatsAppHumanBehaviorOptions _options = new();

    [Theory]
    [InlineData("", 1.5)]
    [InlineData("OK", 1.5)]
    [InlineData("Uma mensagem curta.", 1.5)]
    public void Calculate_ShouldRespectMinimum(string text, double expectedMinSeconds)
    {
        var duration = WhatsAppTypingDurationCalculator.Calculate(text, _options);

        Assert.True(duration.TotalSeconds >= expectedMinSeconds);
    }

    [Fact]
    public void Calculate_LongMessage_ShouldRespectMaximum()
    {
        var text = new string('a', 1000);

        var duration = WhatsAppTypingDurationCalculator.Calculate(text, _options);

        Assert.True(duration.TotalSeconds <= 12);
    }

    [Fact]
    public void Calculate_MediumMessage_ShouldScaleWithLength()
    {
        var shortText = new string('a', 50);
        var longText = new string('a', 200);

        var shortDuration = WhatsAppTypingDurationCalculator.Calculate(shortText, _options);
        var longDuration = WhatsAppTypingDurationCalculator.Calculate(longText, _options);

        Assert.True(longDuration > shortDuration);
    }
}
