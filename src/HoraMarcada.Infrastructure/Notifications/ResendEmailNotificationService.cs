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
                <td style="padding: 0 40px 30px 40px; text-align: center;">
                    <a href="https://horamarcada.com/cancel/{appointment.CancelToken}" style="display: inline-block; padding: 12px 24px; background-color: #ef4444; color: #ffffff; font-size: 14px; font-weight: 600; text-decoration: none; border-radius: 8px; box-shadow: 0 2px 4px rgb(239 68 68 / 0.2);">Cancelar Agendamento</a>
                </td>
            </tr>
            """;

        var htmlBody = GetEmailTemplate(
            store.Name,
            appointment.CustomerName,
            "Seu agendamento foi confirmado com sucesso. Abaixo estão os detalhes da sua reserva:",
            "Confirmado",
            "#ecfdf5",
            "#047857",
            "#4ade80",
            service.Name,
            professional.Name,
            appointment.StartTime.ToString("dd/MM/yyyy HH:mm"),
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
                <td style="padding: 0 0 12px 0;">
                    <span style="font-size: 12px; text-transform: uppercase; letter-spacing: 0.05em; color: #64748b; display: block; margin-bottom: 2px;">Cliente</span>
                    <strong style="font-size: 15px; color: #0f172a;">{appointment.CustomerName} ({appointment.CustomerWhatsApp.Value})</strong>
                </td>
            </tr>
            """;

        var ctaHtml = $"""
            <tr>
                <td style="padding: 0 40px 30px 40px; text-align: center;">
                    <a href="https://horamarcada.com/cancel/{appointment.CancelToken}" style="display: inline-block; padding: 12px 24px; background-color: #ef4444; color: #ffffff; font-size: 14px; font-weight: 600; text-decoration: none; border-radius: 8px; box-shadow: 0 2px 4px rgb(239 68 68 / 0.2);">Cancelar Agendamento</a>
                </td>
            </tr>
            """;

        var htmlBody = GetEmailTemplate(
            store.Name,
            professional.Name,
            "Você tem um novo agendamento marcado. Confira os detalhes abaixo:",
            "Novo Agendamento",
            "#f0fdf4",
            "#166534",
            "#4ade80",
            service.Name,
            professional.Name,
            appointment.StartTime.ToString("dd/MM/yyyy HH:mm"),
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
                <td style="padding: 0 0 12px 0;">
                    <span style="font-size: 12px; text-transform: uppercase; letter-spacing: 0.05em; color: #64748b; display: block; margin-bottom: 2px;">Cliente</span>
                    <strong style="font-size: 15px; color: #0f172a;">{appointment.CustomerName} ({appointment.CustomerWhatsApp.Value})</strong>
                </td>
            </tr>
            """;

        var htmlBody = GetEmailTemplate(
            store.Name,
            store.Name,
            "Um novo agendamento foi realizado em seu estabelecimento. Confira os detalhes:",
            "Novo Agendamento",
            "#f0fdf4",
            "#166534",
            "#4ade80",
            service.Name,
            professional.Name,
            appointment.StartTime.ToString("dd/MM/yyyy HH:mm"),
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
            appointment.StartTime.ToString("dd/MM/yyyy HH:mm"),
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
                <td style="padding: 0 0 12px 0;">
                    <span style="font-size: 12px; text-transform: uppercase; letter-spacing: 0.05em; color: #64748b; display: block; margin-bottom: 2px;">Cliente</span>
                    <strong style="font-size: 15px; color: #0f172a;">{appointment.CustomerName}</strong>
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
            appointment.StartTime.ToString("dd/MM/yyyy HH:mm"),
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
                <td style="padding: 0 0 12px 0;">
                    <span style="font-size: 12px; text-transform: uppercase; letter-spacing: 0.05em; color: #64748b; display: block; margin-bottom: 2px;">Cliente</span>
                    <strong style="font-size: 15px; color: #0f172a;">{appointment.CustomerName}</strong>
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
            appointment.StartTime.ToString("dd/MM/yyyy HH:mm"),
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
            <body style="margin: 0; padding: 0; background-color: #ffffff; font-family: -apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, Helvetica, Arial, sans-serif;">
                <table role="presentation" style="width: 100%; border-collapse: collapse;">
                    <tr>
                        <td align="center" style="padding: 40px 0;">
                            <table role="presentation" style="width: 100%; max-width: 580px; border-collapse: collapse; background-color: #ffffff; border-radius: 16px; overflow: hidden; box-shadow: 0 4px 6px -1px rgb(0 0 0 / 0.05), 0 2px 4px -2px rgb(0 0 0 / 0.05);">
                                <tr>
                                    <td style="height: 6px; background-color: {themeColor};"></td>
                                </tr>
                                <tr>
                                    <td style="padding: 40px 40px 20px 40px; text-align: center;">
                                        <span style="font-size: 24px; font-weight: 700; color: #0f172a; letter-spacing: -0.025em;">{storeName}</span>
                                    </td>
                                </tr>
                                <tr>
                                    <td style="padding: 0 40px 30px 40px; text-align: center;">
                                        <div style="display: inline-block; padding: 6px 16px; background-color: {badgeBg}; border-radius: 9999px;">
                                            <span style="font-size: 14px; font-weight: 600; color: {badgeText};">{statusText}</span>
                                        </div>
                                    </td>
                                </tr>
                                <tr>
                                    <td style="padding: 0 40px 20px 40px;">
                                        <p style="margin: 0; font-size: 16px; line-height: 1.6; color: #334155;">
                                            Olá <strong>{recipientName}</strong>,
                                        </p>
                                        <p style="margin: 8px 0 0 0; font-size: 15px; line-height: 1.6; color: #475569;">
                                            {introText}
                                        </p>
                                    </td>
                                </tr>
                                <tr>
                                    <td style="padding: 0 40px 30px 40px;">
                                        <div style="padding: 24px; background-color: #f8fafc; border-radius: 12px; border-left: 4px solid {themeColor};">
                                            <table role="presentation" style="width: 100%; border-collapse: collapse;">
                                                <tr>
                                                    <td style="padding: 0 0 12px 0;">
                                                        <span style="font-size: 12px; text-transform: uppercase; letter-spacing: 0.05em; color: #64748b; display: block; margin-bottom: 2px;">Serviço</span>
                                                        <strong style="font-size: 15px; color: #0f172a;">{serviceName}</strong>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td style="padding: 0 0 12px 0;">
                                                        <span style="font-size: 12px; text-transform: uppercase; letter-spacing: 0.05em; color: #64748b; display: block; margin-bottom: 2px;">Profissional</span>
                                                        <strong style="font-size: 15px; color: #0f172a;">{professionalName}</strong>
                                                    </td>
                                                </tr>
                                                <tr>
                                                    <td style="padding: 0 0 12px 0;">
                                                        <span style="font-size: 12px; text-transform: uppercase; letter-spacing: 0.05em; color: #64748b; display: block; margin-bottom: 2px;">Data e Hora</span>
                                                        <strong style="font-size: 15px; color: #0f172a;">{dateTime}</strong>
                                                    </td>
                                                </tr>
                                                {extraFieldsHtml}
                                            </table>
                                        </div>
                                    </td>
                                </tr>
                                {ctaButtonHtml}
                                <tr>
                                    <td style="padding: 0 40px 40px 40px; text-align: center; border-top: 1px solid #f1f5f9;">
                                        <p style="margin: 20px 0 0 0; font-size: 12px; color: #94a3b8; line-height: 1.5;">
                                            Este é um e-mail automático enviado pelo <strong>HoraMarcada</strong>.<br>
                                            Por favor, não responda a este e-mail.
                                        </p>
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
}
