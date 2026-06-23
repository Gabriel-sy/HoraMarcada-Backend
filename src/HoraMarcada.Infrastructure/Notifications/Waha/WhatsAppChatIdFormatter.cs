using HoraMarcada.Domain.ValueObjects;

namespace HoraMarcada.Infrastructure.Notifications.Waha;

public static class WhatsAppChatIdFormatter
{
    public static string ToPhoneDigits(PhoneNumber phone)
    {
        return phone.Value.TrimStart('+');
    }

    public static string ToChatId(PhoneNumber phone)
    {
        return $"{ToPhoneDigits(phone)}@c.us";
    }
}
