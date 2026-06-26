using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

using HoraMarcada.Infrastructure.Notifications.Channels.WhatsApp.Client;

namespace HoraMarcada.Infrastructure.Notifications.Channels.WhatsApp.Simulation;

public class WhatsAppHumanBehavior(
    IOptions<WahaOptions> options,
    ILogger<WhatsAppHumanBehavior> logger) : IWhatsAppHumanBehavior
{
    private readonly WhatsAppHumanBehaviorOptions _options = options.Value.HumanBehavior;
    private readonly object _lock = new();
    private DateTimeOffset? _lastMessageSentAt;

    public async Task WaitInterMessageDelayAsync(CancellationToken cancellationToken = default)
    {
        TimeSpan waitTime;
        lock (_lock)
        {
            if (_lastMessageSentAt is null)
            {
                return;
            }

            var requiredGap = SampleGaussianDelay(
                _options.MinInterMessageDelaySeconds,
                _options.MaxInterMessageDelaySeconds);
            var elapsed = DateTimeOffset.UtcNow - _lastMessageSentAt.Value;
            waitTime = requiredGap - elapsed;

            if (waitTime < TimeSpan.Zero)
            {
                waitTime = TimeSpan.Zero;
            }
        }

        if (waitTime > TimeSpan.Zero)
        {
            logger.LogDebug(
                "Waiting {DelaySeconds:F1}s before next WhatsApp message.",
                waitTime.TotalSeconds);
            await Task.Delay(waitTime, cancellationToken);
        }
    }

    public async Task SimulateTypingBeforeSendAsync(
        IWahaClient wahaClient,
        string chatId,
        string text,
        CancellationToken cancellationToken = default)
    {
        var typingDuration = WhatsAppTypingDurationCalculator.Calculate(text, _options);

        logger.LogDebug(
            "Simulating typing for {DurationSeconds:F1}s before sending to {ChatId}.",
            typingDuration.TotalSeconds,
            chatId);

        await wahaClient.SetChatPresenceAsync(chatId, WahaChatPresence.Typing, cancellationToken);

        try
        {
            await Task.Delay(typingDuration, cancellationToken);
        }
        finally
        {
            await wahaClient.SetChatPresenceAsync(chatId, WahaChatPresence.Paused, cancellationToken);
        }
    }

    public async Task WaitFollowUpDelayAsync(CancellationToken cancellationToken = default)
    {
        var delay = SampleGaussianDelay(
            _options.MinFollowUpDelaySeconds,
            _options.MaxFollowUpDelaySeconds);

        logger.LogDebug(
            "Waiting {DelaySeconds:F1}s before follow-up WhatsApp message.",
            delay.TotalSeconds);

        await Task.Delay(delay, cancellationToken);
    }

    public async Task SimulateShortTypingBeforeSendAsync(
        IWahaClient wahaClient,
        string chatId,
        CancellationToken cancellationToken = default)
    {
        var typingDuration = SampleGaussianDelay(
            _options.MinFollowUpTypingDurationSeconds,
            _options.MaxFollowUpTypingDurationSeconds);

        logger.LogDebug(
            "Simulating short typing for {DurationSeconds:F1}s before follow-up to {ChatId}.",
            typingDuration.TotalSeconds,
            chatId);

        await wahaClient.SetChatPresenceAsync(chatId, WahaChatPresence.Typing, cancellationToken);

        try
        {
            await Task.Delay(typingDuration, cancellationToken);
        }
        finally
        {
            await wahaClient.SetChatPresenceAsync(chatId, WahaChatPresence.Paused, cancellationToken);
        }
    }

    public void NotifyMessageSent()
    {
        lock (_lock)
        {
            _lastMessageSentAt = DateTimeOffset.UtcNow;
        }
    }

    private static TimeSpan SampleGaussianDelay(double minSeconds, double maxSeconds)
    {
        var mean = (minSeconds + maxSeconds) / 2d;
        var stdDev = (maxSeconds - minSeconds) / 6d;
        var value = SampleGaussian(mean, stdDev);

        value = Math.Clamp(value, minSeconds, maxSeconds);
        return TimeSpan.FromSeconds(value);
    }

    private static double SampleGaussian(double mean, double stdDev)
    {
        var u1 = 1d - Random.Shared.NextDouble();
        var u2 = Random.Shared.NextDouble();
        var standardNormal = Math.Sqrt(-2d * Math.Log(u1)) * Math.Sin(2d * Math.PI * u2);

        return mean + stdDev * standardNormal;
    }
}
