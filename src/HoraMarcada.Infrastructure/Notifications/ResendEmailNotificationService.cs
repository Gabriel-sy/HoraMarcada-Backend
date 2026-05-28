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
        var htmlBody = $"""
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset="utf-8">
                <meta name="viewport" content="width=device-width, initial-scale=1.0">
                <title>Confirmado - {store.Name}</title>
            </head>
            <body style="{EmailLayout.Body}">
                <table role="presentation" style="{EmailLayout.Table}">
                    <tr>
                        <td align="center" style="padding: 40px 16px;">
                            <table role="presentation" style="{EmailLayout.Container}">
                                <tr>
                                    <td style="{EmailLayout.Header}">
                                        <table role="presentation" style="{EmailLayout.Table}">
                                            <tr>
                                                <td>
                                                    <span style="{EmailLayout.StoreName}">{store.Name}</span>
                                                </td>
                                                <td align="right">
                                                    <span style="{EmailLayout.BadgeBase} background-color: #f0f7e8; color: #5b8a2a;">Confirmado</span>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                                <tr>
                                    <td style="{EmailLayout.Content}">
                                        <p style="{EmailLayout.Greeting}">Olá, {appointment.CustomerName}</p>
                                        <p style="{EmailLayout.IntroText}">Seu agendamento foi confirmado com sucesso. Abaixo estão os detalhes da sua reserva:</p>
                                    </td>
                                </tr>
                                <tr>
                                    <td style="padding: 0 0 24px 0;">
                                        <table role="presentation" style="{EmailLayout.DetailsTable}">
                                            <tr>
                                                <td style="{EmailLayout.DetailItem}">
                                                    <span style="{EmailLayout.DetailLabel}">Serviço</span>
                                                    <span style="{EmailLayout.DetailValue}">{service.Name}</span>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style="{EmailLayout.DetailItem}">
                                                    <span style="{EmailLayout.DetailLabel}">Profissional</span>
                                                    <span style="{EmailLayout.DetailValue}">{professional.Name}</span>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style="{EmailLayout.DetailItem}">
                                                    <span style="{EmailLayout.DetailLabel}">Data e Hora</span>
                                                    <span style="{EmailLayout.DetailValue}">{FormatDateTime(appointment.StartTime)}</span>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                                <tr>
                                    <td style="padding: 8px 0 32px 0;">
                                        <a href="https://horamarcada.com/cancel/{appointment.CancelToken}" style="display: inline-block; padding: 12px 20px; background-color: #ffffff; border: 1px solid #ef4444; color: #ef4444; font-size: 14px; font-weight: 500; text-decoration: none; border-radius: 6px;">Cancelar Agendamento</a>
                                    </td>
                                </tr>
                                {EmailLayout.Footer}
                            </table>
                        </td>
                    </tr>
                </table>
            </body>
            </html>
            """;

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
        var htmlBody = $"""
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset="utf-8">
                <meta name="viewport" content="width=device-width, initial-scale=1.0">
                <title>Novo Agendamento - {store.Name}</title>
            </head>
            <body style="{EmailLayout.Body}">
                <table role="presentation" style="{EmailLayout.Table}">
                    <tr>
                        <td align="center" style="padding: 40px 16px;">
                            <table role="presentation" style="{EmailLayout.Container}">
                                <tr>
                                    <td style="{EmailLayout.Header}">
                                        <table role="presentation" style="{EmailLayout.Table}">
                                            <tr>
                                                <td>
                                                    <span style="{EmailLayout.StoreName}">{store.Name}</span>
                                                </td>
                                                <td align="right">
                                                    <span style="{EmailLayout.BadgeBase} background-color: #f0f7e8; color: #5b8a2a;">Novo Agendamento</span>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                                <tr>
                                    <td style="{EmailLayout.Content}">
                                        <p style="{EmailLayout.Greeting}">Olá, {professional.Name}</p>
                                        <p style="{EmailLayout.IntroText}">Você tem um novo agendamento marcado. Confira os detalhes abaixo:</p>
                                    </td>
                                </tr>
                                <tr>
                                    <td style="padding: 0 0 24px 0;">
                                        <table role="presentation" style="{EmailLayout.DetailsTable}">
                                            <tr>
                                                <td style="{EmailLayout.DetailItem}">
                                                    <span style="{EmailLayout.DetailLabel}">Serviço</span>
                                                    <span style="{EmailLayout.DetailValue}">{service.Name}</span>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style="{EmailLayout.DetailItem}">
                                                    <span style="{EmailLayout.DetailLabel}">Profissional</span>
                                                    <span style="{EmailLayout.DetailValue}">{professional.Name}</span>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style="{EmailLayout.DetailItem}">
                                                    <span style="{EmailLayout.DetailLabel}">Data e Hora</span>
                                                    <span style="{EmailLayout.DetailValue}">{FormatDateTime(appointment.StartTime)}</span>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style="{EmailLayout.DetailItem}">
                                                    <span style="{EmailLayout.DetailLabel}">Cliente</span>
                                                    <span style="{EmailLayout.DetailValue}">{appointment.CustomerName} ({appointment.CustomerWhatsApp.Value})</span>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                                <tr>
                                    <td style="padding: 8px 0 32px 0;">
                                        <a href="https://horamarcada.com/cancel/{appointment.CancelToken}" style="display: inline-block; padding: 12px 20px; background-color: #ffffff; border: 1px solid #ef4444; color: #ef4444; font-size: 14px; font-weight: 500; text-decoration: none; border-radius: 6px;">Cancelar Agendamento</a>
                                    </td>
                                </tr>
                                {EmailLayout.Footer}
                            </table>
                        </td>
                    </tr>
                </table>
            </body>
            </html>
            """;

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
        var htmlBody = $"""
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset="utf-8">
                <meta name="viewport" content="width=device-width, initial-scale=1.0">
                <title>Novo Agendamento - {store.Name}</title>
            </head>
            <body style="{EmailLayout.Body}">
                <table role="presentation" style="{EmailLayout.Table}">
                    <tr>
                        <td align="center" style="padding: 40px 16px;">
                            <table role="presentation" style="{EmailLayout.Container}">
                                <tr>
                                    <td style="{EmailLayout.Header}">
                                        <table role="presentation" style="{EmailLayout.Table}">
                                            <tr>
                                                <td>
                                                    <span style="{EmailLayout.StoreName}">{store.Name}</span>
                                                </td>
                                                <td align="right">
                                                    <span style="{EmailLayout.BadgeBase} background-color: #f0f7e8; color: #5b8a2a;">Novo Agendamento</span>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                                <tr>
                                    <td style="{EmailLayout.Content}">
                                        <p style="{EmailLayout.Greeting}">Olá,</p>
                                        <p style="{EmailLayout.IntroText}">Um novo agendamento foi realizado em seu estabelecimento. Confira os detalhes:</p>
                                    </td>
                                </tr>
                                <tr>
                                    <td style="padding: 0 0 24px 0;">
                                        <table role="presentation" style="{EmailLayout.DetailsTable}">
                                            <tr>
                                                <td style="{EmailLayout.DetailItem}">
                                                    <span style="{EmailLayout.DetailLabel}">Serviço</span>
                                                    <span style="{EmailLayout.DetailValue}">{service.Name}</span>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style="{EmailLayout.DetailItem}">
                                                    <span style="{EmailLayout.DetailLabel}">Profissional</span>
                                                    <span style="{EmailLayout.DetailValue}">{professional.Name}</span>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style="{EmailLayout.DetailItem}">
                                                    <span style="{EmailLayout.DetailLabel}">Data e Hora</span>
                                                    <span style="{EmailLayout.DetailValue}">{FormatDateTime(appointment.StartTime)}</span>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style="{EmailLayout.DetailItem}">
                                                    <span style="{EmailLayout.DetailLabel}">Cliente</span>
                                                    <span style="{EmailLayout.DetailValue}">{appointment.CustomerName} ({appointment.CustomerWhatsApp.Value})</span>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                                {EmailLayout.Footer}
                            </table>
                        </td>
                    </tr>
                </table>
            </body>
            </html>
            """;

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
        var htmlBody = $"""
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset="utf-8">
                <meta name="viewport" content="width=device-width, initial-scale=1.0">
                <title>Cancelado - {store.Name}</title>
            </head>
            <body style="{EmailLayout.Body}">
                <table role="presentation" style="{EmailLayout.Table}">
                    <tr>
                        <td align="center" style="padding: 40px 16px;">
                            <table role="presentation" style="{EmailLayout.Container}">
                                <tr>
                                    <td style="{EmailLayout.Header}">
                                        <table role="presentation" style="{EmailLayout.Table}">
                                            <tr>
                                                <td>
                                                    <span style="{EmailLayout.StoreName}">{store.Name}</span>
                                                </td>
                                                <td align="right">
                                                    <span style="{EmailLayout.BadgeBase} background-color: #fef2f2; color: #991b1b;">Cancelado</span>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                                <tr>
                                    <td style="{EmailLayout.Content}">
                                        <p style="{EmailLayout.Greeting}">Olá, {appointment.CustomerName}</p>
                                        <p style="{EmailLayout.IntroText}">Confirmamos o cancelamento do seu agendamento. Se você não solicitou este cancelamento, entre em contato conosco.</p>
                                    </td>
                                </tr>
                                <tr>
                                    <td style="padding: 0 0 24px 0;">
                                        <table role="presentation" style="{EmailLayout.DetailsTable}">
                                            <tr>
                                                <td style="{EmailLayout.DetailItem}">
                                                    <span style="{EmailLayout.DetailLabel}">Serviço</span>
                                                    <span style="{EmailLayout.DetailValue}">{service.Name}</span>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style="{EmailLayout.DetailItem}">
                                                    <span style="{EmailLayout.DetailLabel}">Profissional</span>
                                                    <span style="{EmailLayout.DetailValue}">{professional.Name}</span>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style="{EmailLayout.DetailItem}">
                                                    <span style="{EmailLayout.DetailLabel}">Data e Hora</span>
                                                    <span style="{EmailLayout.DetailValue}">{FormatDateTime(appointment.StartTime)}</span>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                                {EmailLayout.Footer}
                            </table>
                        </td>
                    </tr>
                </table>
            </body>
            </html>
            """;

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
        var htmlBody = $"""
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset="utf-8">
                <meta name="viewport" content="width=device-width, initial-scale=1.0">
                <title>Cancelado - {store.Name}</title>
            </head>
            <body style="{EmailLayout.Body}">
                <table role="presentation" style="{EmailLayout.Table}">
                    <tr>
                        <td align="center" style="padding: 40px 16px;">
                            <table role="presentation" style="{EmailLayout.Container}">
                                <tr>
                                    <td style="{EmailLayout.Header}">
                                        <table role="presentation" style="{EmailLayout.Table}">
                                            <tr>
                                                <td>
                                                    <span style="{EmailLayout.StoreName}">{store.Name}</span>
                                                </td>
                                                <td align="right">
                                                    <span style="{EmailLayout.BadgeBase} background-color: #fef2f2; color: #991b1b;">Cancelado</span>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                                <tr>
                                    <td style="{EmailLayout.Content}">
                                        <p style="{EmailLayout.Greeting}">Olá, {professional.Name}</p>
                                        <p style="{EmailLayout.IntroText}">O agendamento a seguir foi cancelado e o horário está novamente disponível na sua agenda:</p>
                                    </td>
                                </tr>
                                <tr>
                                    <td style="padding: 0 0 24px 0;">
                                        <table role="presentation" style="{EmailLayout.DetailsTable}">
                                            <tr>
                                                <td style="{EmailLayout.DetailItem}">
                                                    <span style="{EmailLayout.DetailLabel}">Serviço</span>
                                                    <span style="{EmailLayout.DetailValue}">{service.Name}</span>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style="{EmailLayout.DetailItem}">
                                                    <span style="{EmailLayout.DetailLabel}">Profissional</span>
                                                    <span style="{EmailLayout.DetailValue}">{professional.Name}</span>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style="{EmailLayout.DetailItem}">
                                                    <span style="{EmailLayout.DetailLabel}">Data e Hora</span>
                                                    <span style="{EmailLayout.DetailValue}">{FormatDateTime(appointment.StartTime)}</span>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style="{EmailLayout.DetailItem}">
                                                    <span style="{EmailLayout.DetailLabel}">Cliente</span>
                                                    <span style="{EmailLayout.DetailValue}">{appointment.CustomerName}</span>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                                {EmailLayout.Footer}
                            </table>
                        </td>
                    </tr>
                </table>
            </body>
            </html>
            """;

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
        var htmlBody = $"""
            <!DOCTYPE html>
            <html>
            <head>
                <meta charset="utf-8">
                <meta name="viewport" content="width=device-width, initial-scale=1.0">
                <title>Cancelado - {store.Name}</title>
            </head>
            <body style="{EmailLayout.Body}">
                <table role="presentation" style="{EmailLayout.Table}">
                    <tr>
                        <td align="center" style="padding: 40px 16px;">
                            <table role="presentation" style="{EmailLayout.Container}">
                                <tr>
                                    <td style="{EmailLayout.Header}">
                                        <table role="presentation" style="{EmailLayout.Table}">
                                            <tr>
                                                <td>
                                                    <span style="{EmailLayout.StoreName}">{store.Name}</span>
                                                </td>
                                                <td align="right">
                                                    <span style="{EmailLayout.BadgeBase} background-color: #fef2f2; color: #991b1b;">Cancelado</span>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                                <tr>
                                    <td style="{EmailLayout.Content}">
                                        <p style="{EmailLayout.Greeting}">Olá, {store.Name}</p>
                                        <p style="{EmailLayout.IntroText}">Um agendamento em seu estabelecimento foi cancelado. Confira os detalhes:</p>
                                    </td>
                                </tr>
                                <tr>
                                    <td style="padding: 0 0 24px 0;">
                                        <table role="presentation" style="{EmailLayout.DetailsTable}">
                                            <tr>
                                                <td style="{EmailLayout.DetailItem}">
                                                    <span style="{EmailLayout.DetailLabel}">Serviço</span>
                                                    <span style="{EmailLayout.DetailValue}">{service.Name}</span>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style="{EmailLayout.DetailItem}">
                                                    <span style="{EmailLayout.DetailLabel}">Profissional</span>
                                                    <span style="{EmailLayout.DetailValue}">{professional.Name}</span>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style="{EmailLayout.DetailItem}">
                                                    <span style="{EmailLayout.DetailLabel}">Data e Hora</span>
                                                    <span style="{EmailLayout.DetailValue}">{FormatDateTime(appointment.StartTime)}</span>
                                                </td>
                                            </tr>
                                            <tr>
                                                <td style="{EmailLayout.DetailItem}">
                                                    <span style="{EmailLayout.DetailLabel}">Cliente</span>
                                                    <span style="{EmailLayout.DetailValue}">{appointment.CustomerName}</span>
                                                </td>
                                            </tr>
                                        </table>
                                    </td>
                                </tr>
                                {EmailLayout.Footer}
                            </table>
                        </td>
                    </tr>
                </table>
            </body>
            </html>
            """;

        var message = new EmailMessage
        {
            From = _fromEmail,
            Subject = subject,
            HtmlBody = htmlBody
        };
        message.To.Add(store.Email.Value);

        await resend.EmailSendAsync(message, cancellationToken);
    }

    private static string FormatDateTime(DateTimeOffset dateTime)
    {
        return dateTime.ToOffset(TimeSpan.FromHours(-3)).ToString("dd/MM/yyyy HH:mm");
    }
}
