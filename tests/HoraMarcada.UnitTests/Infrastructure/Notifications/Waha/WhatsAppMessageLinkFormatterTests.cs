using HoraMarcada.Infrastructure.Notifications.Waha;
using Xunit;

namespace HoraMarcada.UnitTests.Infrastructure.Notifications.Waha;

public class WhatsAppMessageLinkFormatterTests
{
    [Fact]
    public void ToDisplayUrl_ShouldRemoveScheme()
    {
        var display = WhatsAppMessageLinkFormatter.ToDisplayUrl("https://horamarcada.com/c/abc123");

        Assert.Equal("horamarcada.com/c/abc123", display);
    }

    [Fact]
    public void Format_ShouldUseBoldLabelAndDisplayUrl()
    {
        var formatted = WhatsAppMessageLinkFormatter.Format(
            "Cancelar agendamento",
            "https://horamarcada.com/c/abc123");

        Assert.Equal("*Cancelar agendamento*\nhoramarcada.com/c/abc123", formatted);
    }
}
