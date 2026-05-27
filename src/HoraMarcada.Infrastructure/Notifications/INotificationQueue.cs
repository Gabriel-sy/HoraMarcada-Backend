using System.Threading.Channels;

namespace HoraMarcada.Infrastructure.Notifications;

public interface INotificationQueue
{
    void QueueNotification(NotificationJob job);
    ValueTask<NotificationJob> DequeueAsync(CancellationToken cancellationToken);
}

public class ChannelNotificationQueue : INotificationQueue
{
    private readonly Channel<NotificationJob> _channel;

    public ChannelNotificationQueue()
    {
        var options = new BoundedChannelOptions(10000)
        {
            FullMode = BoundedChannelFullMode.Wait,
            SingleWriter = false,
            SingleReader = true
        };
        _channel = Channel.CreateBounded<NotificationJob>(options);
    }

    public void QueueNotification(NotificationJob job)
    {
        _channel.Writer.TryWrite(job);
    }

    public ValueTask<NotificationJob> DequeueAsync(CancellationToken cancellationToken)
    {
        return _channel.Reader.ReadAsync(cancellationToken);
    }
}
