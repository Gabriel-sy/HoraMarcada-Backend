namespace Marque.Application.Models;

public record BookAppointmentInput(
    Guid StoreId,
    Guid ProfessionalId,
    Guid ServiceId,
    string CustomerName,
    string CustomerWhatsApp,
    DateTimeOffset StartTime);
