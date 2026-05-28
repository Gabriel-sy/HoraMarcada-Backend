namespace HoraMarcada.Infrastructure.Notifications;

public static class EmailLayout
{
    public const string Body = "margin: 0; padding: 0; background-color: #ffffff; font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; -webkit-font-smoothing: antialiased; -moz-osx-font-smoothing: grayscale;";
    public const string Table = "width: 100%; border-collapse: collapse; background-color: #ffffff;";
    public const string Container = "width: 100%; max-width: 520px; border-collapse: collapse; text-align: left;";
    public const string Header = "padding: 0 0 24px 0; border-bottom: 1px solid #e5e5e5;";
    public const string StoreName = "font-size: 18px; font-weight: 700; color: #000000; letter-spacing: -0.02em;";
    public const string BadgeBase = "display: inline-block; padding: 4px 10px; font-size: 11px; font-weight: 600; border-radius: 4px; text-transform: uppercase; letter-spacing: 0.05em;";
    public const string Content = "padding: 32px 0 24px 0;";
    public const string Greeting = "margin: 0; font-size: 16px; line-height: 1.5; color: #000000; font-weight: 600;";
    public const string IntroText = "margin: 12px 0 0 0; font-size: 15px; line-height: 1.6; color: #737373;";
    public const string DetailsTable = "width: 100%; border-collapse: collapse; border-top: 1px solid #e5e5e5;";
    public const string DetailItem = "padding: 14px 0; border-bottom: 1px solid #e5e5e5;";
    public const string DetailLabel = "font-size: 11px; text-transform: uppercase; letter-spacing: 0.05em; color: #737373; display: block; margin-bottom: 4px;";
    public const string DetailValue = "font-size: 15px; color: #000000; font-weight: 500;";

    public const string Footer = """
        <tr>
            <td style="padding: 32px 0 0 0; border-top: 1px solid #e5e5e5;">
                <p style="margin: 0; font-size: 12px; color: #a3a3a3; line-height: 1.5;">Este é um e-mail automático enviado pelo <strong>HoraMarcada</strong>.<br>Por favor, não responda a esta mensagem.</p>
            </td>
        </tr>
        """;
}
