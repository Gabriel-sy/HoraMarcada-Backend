using HoraMarcada.Domain.ValueObjects;
using HoraMarcada.Infrastructure.Notifications.Channels.WhatsApp.Formatting;
using Xunit;

namespace HoraMarcada.UnitTests.Infrastructure.Notifications.Channels.WhatsApp;

public class WhatsAppChatIdFormatterTests
{
    [Fact]
    public void ToPhoneDigits_ShouldStripPlusPrefix()
    {
        var phone = PhoneNumber.Create("+5511987654321");

        var digits = WhatsAppChatIdFormatter.ToPhoneDigits(phone);

        Assert.Equal("5511987654321", digits);
    }

    [Fact]
    public void ToChatId_ShouldAppendCUsSuffix()
    {
        var phone = PhoneNumber.Create("+5511987654321");

        var chatId = WhatsAppChatIdFormatter.ToChatId(phone);

        Assert.Equal("5511987654321@c.us", chatId);
    }
}
