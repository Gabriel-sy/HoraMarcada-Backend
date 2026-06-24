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
            Olá {appointment.CustomerName.Split(' ')[0]}!
            Seu agendamento na {store.Name} foi confirmado:

            📌 Serviço: {service.Name}
            👤 Profissional: {professional.Name}
            📅 Data: {FormatDate(appointment.StartTime)}
            🕐 Horário: {FormatTime(appointment.StartTime)}

            {WhatsAppMessageLinkFormatter.Format("Cancelar agendamento:", cancelUrl)}

            📍 Veja a localização do estabelecimento abaixo.
            """;
    }

    public static string BookingNotificationToProfessional(
        Appointment appointment,
        Service service,
        Professional professional,
        string cancelUrl)
    {
        return $"""
            Olá {professional.Name.Split(' ')[0]}! 
            Você tem um novo agendamento:

            👤 Cliente: {appointment.CustomerName.Split(' ')[0]} ({appointment.CustomerWhatsApp.Value})
            📌 Serviço: {service.Name}
            📅 Data: {FormatDate(appointment.StartTime)}
            🕐 Horário: {FormatTime(appointment.StartTime)}

            {WhatsAppMessageLinkFormatter.Format("Cancelar agendamento:", cancelUrl)}
            """;
    }

    public static string CancellationNoticeToCustomer(
        Appointment appointment,
        Service service,
        Professional professional,
        string storeUrl)
    {
        return $"""
            Olá {appointment.CustomerName.Split(' ')[0]},
            Seu agendamento foi cancelado:

            📌 Serviço: {service.Name} com {professional.Name}
            📅 Data: {FormatDate(appointment.StartTime)}
            🕐 Horário: {FormatTime(appointment.StartTime)}

            {WhatsAppMessageLinkFormatter.Format("Reagendar:", storeUrl)}
            """;
    }

    public static string CancellationNotificationToProfessional(
        Appointment appointment,
        Service service,
        string professionalName)
    {
        return $"""
            Olá {professionalName}! 
            {appointment.CustomerName.Split(' ')[0]} cancelou este agendamento:

            📌 Serviço: {service.Name}
            📅 Data: {FormatDate(appointment.StartTime)}
            🕐 Horário: {FormatTime(appointment.StartTime)}
            """;
    }

    public static string BookingReminder24h(
        Appointment appointment,
        Service service,
        Professional professional,
        string cancelUrl,
        string rescheduleUrl)
    {
        return $"""
            Olá {appointment.CustomerName.Split(' ')[0]}! 
            Lembrete do seu agendamento amanhã:

            📌 Serviço: {service.Name} com {professional.Name}
            📅 Data: {FormatDate(appointment.StartTime)}
            🕐 Horário: {FormatTime(appointment.StartTime)}

            {WhatsAppMessageLinkFormatter.Format("Cancelar agendamento:", cancelUrl)}

            {WhatsAppMessageLinkFormatter.Format("Reagendar:", rescheduleUrl)}
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
