using HoraMarcada.Application.Common;
using HoraMarcada.Domain.Entities;
using Microsoft.Extensions.Configuration;
using Resend;

namespace HoraMarcada.Infrastructure.Notifications;

public class ResendEmailNotificationService(IResend resend, IConfiguration configuration) : INotificationService
{
    private readonly string _fromEmail = configuration["Resend:FromEmail"] ?? "onboarding@resend.dev";

    public async Task SendBookingConfirmationToCustomerAsync(
        Appointment appointment,
        Store store,
        Service service,
        Professional professional,
        CancellationToken cancellationToken = default)
    {
        var subject = $"Agendamento Confirmado - {store.Name}";
        var ctaHtml = $"""
            <tr>
                <td style="padding: 8px 0 32px 0;">
                    <a href="https://horamarcada.com/cancel/{appointment.CancelToken}" style="display: inline-block; padding: 12px 20px; background-color: #ffffff; border: 1px solid #ef4444; color: #ef4444; font-size: 14px; font-weight: 500; text-decoration: none; border-radius: 6px;">Cancelar Agendamento</a>
                </td>
            </tr>
            """;

        var htmlBody = GetEmailTemplate(
            store.Name,
            appointment.CustomerName,
            "Seu agendamento foi confirmado com sucesso. Abaixo estão os detalhes da sua reserva:",
            "Confirmado",
            "#f0f7e8",
            "#5b8a2a",
            "#a3d271",
            service.Name,
            professional.Name,
            FormatDateTime(appointment.StartTime),
            "",
            ctaHtml
        );

        var message = new EmailMessage
        {
            From = _fromEmail,
            Subject = subject,
            HtmlBody = htmlBody
        };
        message.To.Add(appointment.CustomerEmail.Value);

        await resend.EmailSendAsync(message, cancellationToken);
    }

    public async Task SendBookingNotificationToProfessionalAsync(
        Appointment appointment,
        Store store,
        Service service,
        Professional professional,
        CancellationToken cancellationToken = default)
    {
        var subject = $"Novo Agendamento - {store.Name}";
        var extraHtml = $"""
            <tr>
                <td style="padding: 14px 0; border-bottom: 1px solid #e5e5e5;">
                    <span style="font-size: 11px; text-transform: uppercase; letter-spacing: 0.05em; color: #737373; display: block; margin-bottom: 4px;">Cliente</span>
                    <span style="font-size: 15px; color: #000000; font-weight: 500;">{appointment.CustomerName} ({appointment.CustomerWhatsApp.Value})</span>
                </td>
            </tr>
            """;

        var ctaHtml = $"""
            <tr>
                <td style="padding: 8px 0 32px 0;">
                    <a href="https://horamarcada.com/cancel/{appointment.CancelToken}" style="display: inline-block; padding: 12px 20px; background-color: #ffffff; border: 1px solid #ef4444; color: #ef4444; font-size: 14px; font-weight: 500; text-decoration: none; border-radius: 6px;">Cancelar Agendamento</a>
                </td>
            </tr>
            """;

        var htmlBody = GetEmailTemplate(
            store.Name,
            professional.Name,
            "Você tem um novo agendamento marcado. Confira os detalhes abaixo:",
            "Novo Agendamento",
            "#f0f7e8",
            "#5b8a2a",
            "#a3d271",
            service.Name,
            professional.Name,
            FormatDateTime(appointment.StartTime),
            extraHtml,
            ctaHtml
        );

        var message = new EmailMessage
        {
            From = _fromEmail,
            Subject = subject,
            HtmlBody = htmlBody
        };
        message.To.Add(professional.Email!.Value);

        await resend.EmailSendAsync(message, cancellationToken);
    }

    public async Task SendBookingNotificationToStoreAsync(
        Appointment appointment,
        Store store,
        Service service,
        Professional professional,
        CancellationToken cancellationToken = default)
    {
        var subject = $"Novo Agendamento em {store.Name}";
        var extraHtml = $"""
            <tr>
                <td style="padding: 14px 0; border-bottom: 1px solid #e5e5e5;">
                    <span style="font-size: 11px; text-transform: uppercase; letter-spacing: 0.05em; color: #737373; display: block; margin-bottom: 4px;">Cliente</span>
                    <span style="font-size: 15px; color: #000000; font-weight: 500;">{appointment.CustomerName} ({appointment.CustomerWhatsApp.Value})</span>
                </td>
            </tr>
            """;

        var htmlBody = GetEmailTemplate(
            store.Name,
            store.Name,
            "Um novo agendamento foi realizado em seu estabelecimento. Confira os detalhes:",
            "Novo Agendamento",
            "#f0f7e8",
            "#5b8a2a",
            "#a3d271",
            service.Name,
            professional.Name,
            FormatDateTime(appointment.StartTime),
            extraHtml,
            ""
        );

        var message = new EmailMessage
        {
            From = _fromEmail,
            Subject = subject,
            HtmlBody = htmlBody
        };
        message.To.Add(store.Email.Value);

        await resend.EmailSendAsync(message, cancellationToken);
    }

    public async Task SendCancellationNoticeToCustomerAsync(
        Appointment appointment,
        Store store,
        Service service,
        Professional professional,
        CancellationToken cancellationToken = default)
    {
        var subject = $"Agendamento Cancelado - {store.Name}";
        var htmlBody = GetEmailTemplate(
            store.Name,
            appointment.CustomerName,
            "Confirmamos o cancelamento do seu agendamento. Se você não solicitou este cancelamento, entre em contato conosco.",
            "Cancelado",
            "#fef2f2",
            "#991b1b",
            "#ef4444",
            service.Name,
            professional.Name,
            FormatDateTime(appointment.StartTime),
            "",
            ""
        );

        var message = new EmailMessage
        {
            From = _fromEmail,
            Subject = subject,
            HtmlBody = htmlBody
        };
        message.To.Add(appointment.CustomerEmail.Value);

        await resend.EmailSendAsync(message, cancellationToken);
    }

    public async Task SendCancellationNotificationToProfessionalAsync(
        Appointment appointment,
        Store store,
        Service service,
        Professional professional,
        CancellationToken cancellationToken = default)
    {
        var subject = $"Agendamento Cancelado - {store.Name}";
        var extraHtml = $"""
            <tr>
                <td style="padding: 14px 0; border-bottom: 1px solid #e5e5e5;">
                    <span style="font-size: 11px; text-transform: uppercase; letter-spacing: 0.05em; color: #737373; display: block; margin-bottom: 4px;">Cliente</span>
                    <span style="font-size: 15px; color: #000000; font-weight: 500;">{appointment.CustomerName}</span>
                </td>
            </tr>
            """;

        var htmlBody = GetEmailTemplate(
            store.Name,
            professional.Name,
            "O agendamento a seguir foi cancelado e o horário está novamente disponível na sua agenda:",
            "Cancelado",
            "#fef2f2",
            "#991b1b",
            "#ef4444",
            service.Name,
            professional.Name,
            FormatDateTime(appointment.StartTime),
            extraHtml,
            ""
        );

        var message = new EmailMessage
        {
            From = _fromEmail,
            Subject = subject,
            HtmlBody = htmlBody
        };
        message.To.Add(professional.Email!.Value);

        await resend.EmailSendAsync(message, cancellationToken);
    }

    public async Task SendCancellationNotificationToStoreAsync(
        Appointment appointment,
        Store store,
        Service service,
        Professional professional,
        CancellationToken cancellationToken = default)
    {
        var subject = $"Agendamento Cancelado em {store.Name}";
        var extraHtml = $"""
            <tr>
                <td style="padding: 14px 0; border-bottom: 1px solid #e5e5e5;">
                    <span style="font-size: 11px; text-transform: uppercase; letter-spacing: 0.05em; color: #737373; display: block; margin-bottom: 4px;">Cliente</span>
                    <span style="font-size: 15px; color: #000000; font-weight: 500;">{appointment.CustomerName}</span>
                </td>
            </tr>
            """;

        var htmlBody = GetEmailTemplate(
            store.Name,
            store.Name,
            "Um agendamento em seu estabelecimento foi cancelado. Confira os detalhes:",
            "Cancelado",
            "#fef2f2",
            "#991b1b",
            "#ef4444",
            service.Name,
            professional.Name,
            FormatDateTime(appointment.StartTime),
            extraHtml,
            ""
        );

        var message = new EmailMessage
        {
            From = _fromEmail,
            Subject = subject,
            HtmlBody = htmlBody
        };
        message.To.Add(store.Email.Value);

        await resend.EmailSendAsync(message, cancellationToken);
    }

    private static string GetEmailTemplate(
        string storeName,
        string recipientName,
        string introText,
        string statusText,
        string badgeBg,
        string badgeText,
        string themeColor,
        string serviceName,
        string professionalName,
        string dateTime,
        string extraFieldsHtml,
        string ctaButtonHtml)
    {
        return $"""
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset="utf-8">
                <meta name="viewport" content="width=device-width, initial-scale=1.0">
                <title>{statusText} - {storeName}</title>
            </head>
            <body style="margin: 0; padding: 0; background-color: #ffffff; font-family: 'Inter', -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif; -webkit-font-smoothing: antialiased; -moz-osx-font-smoothing: grayscale;">
                <table role="presentation" style="width: 100%; border-collapse: collapse; background-color: #ffffff;">
                    <tr>
                        <td align="center" style="padding: 40px 16px;">
                            <table role="presentation" style="width: 100%; max-width: 520px; border-collapse: collapse; text-align: left;">
                                <tr>
                                    <td style="padding: 0 0 24px 0; border-bottom: 1px solid #e5e5e5;">
                                        <table role="presentation" style="width: 100%; border-collapse: collapse;">
                                            <tr>
                                                <td>
                                                    <span style="font-size: 18px; font-weight: 700; color: #000000; letter-spacing: -0.02em;">{storeName}</span>
                                                </td>
                                                <td align="right">
                                                    <span style="display: inline-block; padding: 4px 10px; background-color: {badgeBg}; color: {badgeText}; font-size: 11px; font-weight: 600; border-radius: 4px; text-transform: uppercase; letter-spacing: 0.05em;">{statusText}</span>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                                <tr>
                                    <td style="padding: 32px 0 24px 0;">
                                        <p style="margin: 0; font-size: 16px; line-height: 1.5; color: #000000; font-weight: 600;">Olá, {recipientName}</p>
                                        <p style="margin: 12px 0 0 0; font-size: 15px; line-height: 1.6; color: #737373;">{introText}</p>
                                    </td>
                                </tr>
                                <tr>
                                    <td style="padding: 0 0 24px 0;">
                                        <table role="presentation" style="width: 100%; border-collapse: collapse; border-top: 1px solid #e5e5e5;">
                                            <tr>
                                                <td style="padding: 14px 0; border-bottom: 1px solid #e5e5e5;">
                                                    <span style="font-size: 11px; text-transform: uppercase; letter-spacing: 0.05em; color: #737373; display: block; margin-bottom: 4px;">Serviço</span>
                                                    <span style="font-size: 15px; color: #000000; font-weight: 500;">{serviceName}</span>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style="padding: 14px 0; border-bottom: 1px solid #e5e5e5;">
                                                    <span style="font-size: 11px; text-transform: uppercase; letter-spacing: 0.05em; color: #737373; display: block; margin-bottom: 4px;">Profissional</span>
                                                    <span style="font-size: 15px; color: #000000; font-weight: 500;">{professionalName}</span>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style="padding: 14px 0; border-bottom: 1px solid #e5e5e5;">
                                                    <span style="font-size: 11px; text-transform: uppercase; letter-spacing: 0.05em; color: #737373; display: block; margin-bottom: 4px;">Data e Hora</span>
                                                    <span style="font-size: 15px; color: #000000; font-weight: 500;">{dateTime}</span>
                                                </td>
                                            </tr>
                                            {extraFieldsHtml}
                                        </table>
                                    </td>
                                </tr>
                                {ctaButtonHtml}
                                <tr>
                                    <td style="padding: 32px 0 0 0; border-top: 1px solid #e5e5e5;">
                                        <p style="margin: 0; font-size: 12px; color: #a3a3a3; line-height: 1.5;">Este é um e-mail automático enviado pelo <strong>HoraMarcada</strong>.<br>Por favor, não responda a esta mensagem.</p>
                                    </td>
                                </tr>
                            </table>
                        </td>
                    </tr>
                </table>
            </body>
            </html>
            """;
    }

    private static string FormatDateTime(DateTimeOffset dateTime)
    {
        return dateTime.ToOffset(TimeSpan.FromHours(-3)).ToString("dd/MM/yyyy HH:mm");
    }
}
