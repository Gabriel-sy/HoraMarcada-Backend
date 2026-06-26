using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using HoraMarcada.Infrastructure.Notifications.Exceptions;

namespace HoraMarcada.Infrastructure.Notifications.Channels.WhatsApp.Client;

public class WahaClient(
    HttpClient httpClient,
    IOptions<WahaOptions> options,
    ILogger<WahaClient> logger) : IWahaClient
{
    private static readonly TimeSpan RateLimitRetryDelay = TimeSpan.FromSeconds(30);
    private readonly WahaOptions _options = options.Value;

    public async Task<WahaSessionStatus> GetSessionStatusAsync(CancellationToken cancellationToken = default)
    {
        var response = await SendWithRateLimitRetryAsync(
            () => httpClient.GetAsync($"api/sessions/{_options.SessionName}", cancellationToken),
            cancellationToken);

        if (response.StatusCode == HttpStatusCode.NotFound)
        {
            logger.LogError("WAHA session '{SessionName}' not found.", _options.SessionName);
            throw new NotificationNonRetryableException($"WAHA session '{_options.SessionName}' not found.");
        }

        await EnsureSuccessOrThrowAsync(response, cancellationToken);

        var session = await response.Content.ReadFromJsonAsync<WahaSessionResponse>(cancellationToken);
        return MapSessionStatus(session?.Status);
    }

    public async Task<WahaContactCheckResult> CheckContactExistsAsync(
        string phoneDigits,
        CancellationToken cancellationToken = default)
    {
        var response = await SendWithRateLimitRetryAsync(
            () => httpClient.GetAsync(
                $"api/contacts/check-exists?phone={Uri.EscapeDataString(phoneDigits)}&session={Uri.EscapeDataString(_options.SessionName)}",
                cancellationToken),
            cancellationToken);

        await EnsureSuccessOrThrowAsync(response, cancellationToken);

        var result = await response.Content.ReadFromJsonAsync<WahaCheckExistsResponse>(cancellationToken);
        return new WahaContactCheckResult(result?.NumberExists ?? false, result?.ChatId);
    }

    public async Task SendTextAsync(string chatId, string text, CancellationToken cancellationToken = default)
    {
        var payload = new WahaSendTextRequest(_options.SessionName, chatId, text, true);

        var response = await SendWithRateLimitRetryAsync(
            () => httpClient.PostAsJsonAsync("api/sendText", payload, cancellationToken),
            cancellationToken);

        await EnsureSuccessOrThrowAsync(response, cancellationToken);
    }

    public async Task SetChatPresenceAsync(
        string chatId,
        WahaChatPresence presence,
        CancellationToken cancellationToken = default)
    {
        var payload = new WahaSetPresenceRequest(chatId, MapPresence(presence));

        var response = await SendWithRateLimitRetryAsync(
            () => httpClient.PostAsJsonAsync(
                $"api/{Uri.EscapeDataString(_options.SessionName)}/presence",
                payload,
                cancellationToken),
            cancellationToken);

        await EnsureSuccessOrThrowAsync(response, cancellationToken);
    }

    public async Task SendLocationAsync(
        string chatId,
        double latitude,
        double longitude,
        CancellationToken cancellationToken = default)
    {
        var payload = new WahaSendLocationRequest(
            _options.SessionName,
            chatId,
            latitude,
            longitude);

        var response = await SendWithRateLimitRetryAsync(
            () => httpClient.PostAsJsonAsync("api/sendLocation", payload, cancellationToken),
            cancellationToken);

        await EnsureSuccessOrThrowAsync(response, cancellationToken);
    }

    private async Task<HttpResponseMessage> SendWithRateLimitRetryAsync(
        Func<Task<HttpResponseMessage>> send,
        CancellationToken cancellationToken)
    {
        var response = await send();

        if (response.StatusCode != HttpStatusCode.TooManyRequests)
        {
            return response;
        }

        logger.LogWarning("WAHA rate limit hit. Waiting {DelaySeconds}s before retry.", RateLimitRetryDelay.TotalSeconds);
        response.Dispose();
        await Task.Delay(RateLimitRetryDelay, cancellationToken);
        return await send();
    }

    private async Task EnsureSuccessOrThrowAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var body = await response.Content.ReadAsStringAsync(cancellationToken);

        if ((int)response.StatusCode >= 500)
        {
            throw new HttpRequestException(
                $"WAHA returned {(int)response.StatusCode}: {body}",
                null,
                response.StatusCode);
        }

        logger.LogError("WAHA request failed with status {StatusCode}: {Body}", (int)response.StatusCode, body);
        throw new NotificationNonRetryableException($"WAHA returned {(int)response.StatusCode}: {body}");
    }

    private static string MapPresence(WahaChatPresence presence)
    {
        return presence switch
        {
            WahaChatPresence.Typing => "typing",
            WahaChatPresence.Paused => "paused",
            _ => throw new ArgumentOutOfRangeException(nameof(presence), presence, null)
        };
    }

    private static WahaSessionStatus MapSessionStatus(string? status)
    {
        return status?.ToUpperInvariant() switch
        {
            "WORKING" => WahaSessionStatus.Working,
            "FAILED" => WahaSessionStatus.Failed,
            "SCAN_QR_CODE" => WahaSessionStatus.ScanQrCode,
            "STARTING" => WahaSessionStatus.Starting,
            "STOPPED" => WahaSessionStatus.Stopped,
            _ => WahaSessionStatus.Unknown
        };
    }

    private sealed record WahaSendTextRequest(
        string Session,
        string ChatId,
        string Text,
        bool LinkPreview);

    private sealed record WahaSetPresenceRequest(
        string ChatId,
        string Presence);

    private sealed record WahaSendLocationRequest(
        string Session,
        string ChatId,
        double Latitude,
        double Longitude);

    private sealed class WahaSessionResponse
    {
        [JsonPropertyName("status")]
        public string? Status { get; init; }
    }

    private sealed class WahaCheckExistsResponse
    {
        [JsonPropertyName("numberExists")]
        public bool NumberExists { get; init; }

        [JsonPropertyName("chatId")]
        public string? ChatId { get; init; }
    }
}
