namespace HoraMarcada.Infrastructure.Notifications.Waha;

public interface IWahaClient
{
    Task<WahaSessionStatus> GetSessionStatusAsync(CancellationToken cancellationToken = default);

    Task<WahaContactCheckResult> CheckContactExistsAsync(string phoneDigits, CancellationToken cancellationToken = default);

    Task SendTextAsync(string chatId, string text, CancellationToken cancellationToken = default);

    Task SetChatPresenceAsync(
        string chatId,
        WahaChatPresence presence,
        CancellationToken cancellationToken = default);

    Task SendLocationAsync(
        string chatId,
        double latitude,
        double longitude,
        CancellationToken cancellationToken = default);
}

public sealed record WahaContactCheckResult(bool NumberExists, string? ChatId);
