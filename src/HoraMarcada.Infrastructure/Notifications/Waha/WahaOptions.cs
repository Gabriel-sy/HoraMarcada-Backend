namespace HoraMarcada.Infrastructure.Notifications.Waha;

public class WahaOptions
{
    public const string SectionName = "Waha";

    public string BaseUrl { get; set; } = "http://localhost:3000";

    public string ApiKey { get; set; } = string.Empty;

    public string SessionName { get; set; } = "default";

    public WhatsAppHumanBehaviorOptions HumanBehavior { get; set; } = new();
}
