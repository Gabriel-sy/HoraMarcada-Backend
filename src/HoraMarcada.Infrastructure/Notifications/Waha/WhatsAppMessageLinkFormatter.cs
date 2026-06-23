namespace HoraMarcada.Infrastructure.Notifications.Waha;

public static class WhatsAppMessageLinkFormatter
{
    public static string Format(string label, string url)
    {
        return $"*{label}*\n{ToDisplayUrl(url)}";
    }

    public static string ToDisplayUrl(string url)
    {
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uri))
        {
            return url;
        }

        var display = uri.Host + uri.PathAndQuery;
        if (!string.IsNullOrEmpty(uri.Fragment))
        {
            display += uri.Fragment;
        }

        return display.TrimEnd('/');
    }
}
