namespace HoraMarcada.Infrastructure.Notifications;

public class NotificationNonRetryableException : Exception
{
    public NotificationNonRetryableException(string message) : base(message)
    {
    }

    public NotificationNonRetryableException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
