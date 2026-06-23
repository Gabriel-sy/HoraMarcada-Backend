namespace HoraMarcada.Infrastructure.Notifications.Waha;

public interface IWhatsAppHumanBehavior
{
    Task WaitInterMessageDelayAsync(CancellationToken cancellationToken = default);

    Task SimulateTypingBeforeSendAsync(
        IWahaClient wahaClient,
        string chatId,
        string text,
        CancellationToken cancellationToken = default);

    Task WaitFollowUpDelayAsync(CancellationToken cancellationToken = default);

    Task SimulateShortTypingBeforeSendAsync(
        IWahaClient wahaClient,
        string chatId,
        CancellationToken cancellationToken = default);

    void NotifyMessageSent();
}
