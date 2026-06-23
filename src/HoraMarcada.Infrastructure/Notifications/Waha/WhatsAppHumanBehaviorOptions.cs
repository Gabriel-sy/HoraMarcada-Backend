namespace HoraMarcada.Infrastructure.Notifications.Waha;

public class WhatsAppHumanBehaviorOptions
{
    public double MinInterMessageDelaySeconds { get; set; } = 3;

    public double MaxInterMessageDelaySeconds { get; set; } = 8;

    public double MinTypingDurationSeconds { get; set; } = 1.5;

    public double MaxTypingDurationSeconds { get; set; } = 12;

    public double MillisecondsPerCharacter { get; set; } = 40;

    public double TypingDurationVarianceFactor { get; set; } = 0.25;

    public double MinFollowUpDelaySeconds { get; set; } = 1.2;

    public double MaxFollowUpDelaySeconds { get; set; } = 2.5;

    public double MinFollowUpTypingDurationSeconds { get; set; } = 1;

    public double MaxFollowUpTypingDurationSeconds { get; set; } = 2;
}
