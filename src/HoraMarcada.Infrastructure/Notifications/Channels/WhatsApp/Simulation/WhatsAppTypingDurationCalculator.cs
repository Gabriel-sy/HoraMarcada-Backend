namespace HoraMarcada.Infrastructure.Notifications.Channels.WhatsApp.Simulation;

public static class WhatsAppTypingDurationCalculator
{
    public static TimeSpan Calculate(string text, WhatsAppHumanBehaviorOptions options)
    {
        var baseSeconds = text.Length * options.MillisecondsPerCharacter / 1000d;
        var variance = 1d + (Random.Shared.NextDouble() * 2d - 1d) * options.TypingDurationVarianceFactor;
        var seconds = baseSeconds * variance;

        seconds = Math.Clamp(
            seconds,
            options.MinTypingDurationSeconds,
            options.MaxTypingDurationSeconds);

        return TimeSpan.FromSeconds(seconds);
    }
}
