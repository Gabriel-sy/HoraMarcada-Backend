namespace HoraMarcada.Infrastructure.Notifications.Common;

public static class AppointmentNotificationUrls
{
    public static string BuildCancelUrl(string frontendUrl, string token)
    {
        return $"{frontendUrl.TrimEnd('/')}/c/{token}";
    }

    public static string BuildRescheduleUrl(string frontendUrl, string token)
    {
        return $"{frontendUrl.TrimEnd('/')}/r/{token}";
    }

    public static string BuildConfirmUrl(string frontendUrl, string token)
    {
        return $"{frontendUrl.TrimEnd('/')}/co/{token}";
    }
}
