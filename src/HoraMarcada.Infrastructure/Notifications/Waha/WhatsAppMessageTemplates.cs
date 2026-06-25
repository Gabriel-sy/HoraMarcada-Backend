using HoraMarcada.Domain.Entities;

namespace HoraMarcada.Infrastructure.Notifications.Waha;

public static class WhatsAppMessageTemplates
{
    public static string BookingConfirmationToCustomer(
        Appointment appointment,
        Store store,
        Service service,
        Professional professional,
        string cancelUrl)
    {
        return $"""
            Olá, {appointment.CustomerName.Split(' ')[0]}!

            Seu agendamento na {store.Name} foi confirmado com sucesso. 🎉

            Aqui estão os detalhes do seu compromisso:
            👉 {service.Name} com {professional.Name}
            👉 Dia {FormatDate(appointment.StartTime)} às {FormatTime(appointment.StartTime)}h

            📍 A localização do estabelecimento está logo abaixo.
            Caso não consiga comparecer, utilize o link a seguir para cancelar o agendamento:
            {WhatsAppMessageLinkFormatter.ToDisplayUrl(cancelUrl)}

            Muito obrigado!
            """;
    }

    public static string BookingNotificationToProfessional(
        Appointment appointment,
        Service service,
        Professional professional,
        string cancelUrl)
    {
        return $"""
            Olá, {professional.Name.Split(' ')[0]}!

            Você tem um novo agendamento:

            👤 Cliente: {appointment.CustomerName.Split(' ')[0]} ({appointment.CustomerWhatsApp.Value})
            📌 Serviço: {service.Name}
            📅 Data: {FormatDate(appointment.StartTime)} às {FormatTime(appointment.StartTime)}h

            Caso precise realizar o cancelamento, utilize o link a seguir:
            {WhatsAppMessageLinkFormatter.ToDisplayUrl(cancelUrl)}
            """;
    }

    public static string CancellationNoticeToCustomer(
        Appointment appointment,
        Service service,
        Professional professional,
        string storeUrl)
    {
        return $"""
            Olá, {appointment.CustomerName.Split(' ')[0]}!

            Seu agendamento foi cancelado:

            📌 Serviço: {service.Name} com {professional.Name}
            📅 Data: {FormatDate(appointment.StartTime)} às {FormatTime(appointment.StartTime)}h

            Para realizar um novo agendamento, utilize o link a seguir:
            {WhatsAppMessageLinkFormatter.ToDisplayUrl(storeUrl)}
            """;
    }

    public static string CancellationNotificationToProfessional(
        Appointment appointment,
        Service service,
        string professionalName)
    {
        return $"""
            Olá, {professionalName.Split(' ')[0]}!

            O agendamento do(a) cliente {appointment.CustomerName.Split(' ')[0]} foi cancelado:

            📌 Serviço: {service.Name}
            📅 Data: {FormatDate(appointment.StartTime)} às {FormatTime(appointment.StartTime)}h
            """;
    }

    public static string BookingReminder24h(
        Appointment appointment,
        Service service,
        Professional professional,
        string storeName,
        string cancelUrl,
        string rescheduleUrl)
    {
        return $"""
            Olá, {appointment.CustomerName.Split(' ')[0]}!

            Este é um lembrete do seu agendamento na {storeName}.

            📌 Serviço: {service.Name} com {professional.Name}
            📅 Data: {FormatDate(appointment.StartTime)} às {FormatTime(appointment.StartTime)}h

            Caso precise alterar ou cancelar o seu horário, utilize os links abaixo:
            Reagendar: {WhatsAppMessageLinkFormatter.ToDisplayUrl(rescheduleUrl)}
            Cancelar: {WhatsAppMessageLinkFormatter.ToDisplayUrl(cancelUrl)}

            Agradecemos a preferência!
            """;
    }

    public static string FormatDate(DateTimeOffset dateTime)
    {
        return dateTime.ToOffset(TimeSpan.FromHours(-3)).ToString("dd/MM/yyyy");
    }

    public static string FormatTime(DateTimeOffset dateTime)
    {
        return dateTime.ToOffset(TimeSpan.FromHours(-3)).ToString("HH:mm");
    }
}

