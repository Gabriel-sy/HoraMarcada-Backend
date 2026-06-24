using HoraMarcada.Domain.Enums;
using HoraMarcada.Domain.Exceptions;
using HoraMarcada.Domain.ValueObjects;

namespace HoraMarcada.Domain.Entities;

public sealed class Appointment
{
    public Guid Id { get; private set; }
    public Guid StoreId { get; private set; }
    public Guid ProfessionalId { get; private set; }
    public Guid ServiceId { get; private set; }
    public Guid CustomerId { get; private set; }
    public Customer Customer { get; private set; } = null!;

    private string _customerName = string.Empty;
    private PhoneNumber _customerWhatsApp = null!;

    public string CustomerName => Customer?.Name ?? _customerName;
    public PhoneNumber CustomerWhatsApp => Customer?.WhatsApp ?? _customerWhatsApp;

    public DateTimeOffset StartTime { get; private set; }
    public DateTimeOffset EndTime { get; private set; }
    public AppointmentStatus Status { get; private set; }
    public string? CancelToken { get; private set; }
    public bool CancelTokenUsed { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }

    private Appointment()
    {
    }

    public static Appointment Create(
        Guid storeId,
        Guid professionalId,
        Guid serviceId,
        string customerName,
        string customerWhatsApp,
        DateTimeOffset startTime,
        DateTimeOffset endTime,
        AppointmentStatus status = AppointmentStatus.Confirmed,
        string? cancelToken = null,
        Guid? customerId = null)
    {
        if (storeId == Guid.Empty)
            throw new DomainValidationException("Store id is required.");

        if (professionalId == Guid.Empty)
            throw new DomainValidationException("Professional id is required.");

        if (serviceId == Guid.Empty)
            throw new DomainValidationException("Service id is required.");

        if (string.IsNullOrWhiteSpace(customerName))
            throw new DomainValidationException("Customer name is required.");

        if (endTime <= startTime)
            throw new DomainValidationException("End time must be after start time.");

        return new Appointment
        {
            Id = Guid.NewGuid(),
            StoreId = storeId,
            ProfessionalId = professionalId,
            ServiceId = serviceId,
            CustomerId = customerId ?? Guid.Empty,
            _customerName = customerName.Trim(),
            _customerWhatsApp = PhoneNumber.Create(customerWhatsApp),
            StartTime = startTime.ToUniversalTime(),
            EndTime = endTime.ToUniversalTime(),
            Status = status,
            CancelToken = string.IsNullOrWhiteSpace(cancelToken) ? null : cancelToken.Trim(),
            CancelTokenUsed = false,
            CreatedAt = DateTimeOffset.UtcNow
        };
    }

    public void Cancel()
    {
        if (Status == AppointmentStatus.Completed)
            throw new DomainValidationException("Cannot cancel a completed appointment.");
        Status = AppointmentStatus.Canceled;
    }

    public void Complete()
    {
        if (Status == AppointmentStatus.Canceled)
            throw new DomainValidationException("Cannot complete a canceled appointment.");
        Status = AppointmentStatus.Completed;
    }

    public void ConfirmPresence()
    {
        if (Status != AppointmentStatus.Confirmed)
            throw new DomainValidationException("Only confirmed appointments can have presence confirmed by customer.");
        Status = AppointmentStatus.ConfirmedByUser;
    }

    public void MarkCancelTokenUsed()
    {
        CancelTokenUsed = true;
    }
}
