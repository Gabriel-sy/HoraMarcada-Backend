using HoraMarcada.Application.Common;
using HoraMarcada.Application.Exceptions;
using HoraMarcada.Application.Models;
using HoraMarcada.Application.Repositories;
using HoraMarcada.Application.UseCases.Availability;
using HoraMarcada.Domain.Entities;
using HoraMarcada.Domain.Exceptions;

namespace HoraMarcada.Application.UseCases.Appointments;

public class BookAppointmentUseCase(
    IAppointmentRepository appointmentRepository,
    IStoreRepository storeRepository,
    IServiceRepository serviceRepository,
    IProfessionalRepository professionalRepository,
    GetAvailableSlotsUseCase getAvailableSlotsUseCase,
    INotificationService notificationService,
    IBackgroundJobService backgroundJobService,
    ITimeProvider timeProvider,
    ICustomerRepository customerRepository)
{
    public async Task<Appointment> ExecuteAsync(BookAppointmentInput input, CancellationToken cancellationToken = default)
    {
        if (input.StartTime < timeProvider.UtcNow)
            throw new DomainValidationException("Cannot book an appointment in the past.");

        var store = await storeRepository.GetByIdAsync(input.StoreId, cancellationToken);
        if (store == null)
            throw new NotFoundException($"Store with ID {input.StoreId} not found.");

        if (!store.Active)
            throw new ConflictException($"Store {input.StoreId} is inactive.");

        var service = await serviceRepository.GetByIdAsync(input.ServiceId, cancellationToken);
        if (service == null || service.StoreId != input.StoreId)
            throw new NotFoundException($"Service with ID {input.ServiceId} not found in store {input.StoreId}.");

        if (!service.Active)
            throw new ConflictException($"Service {input.ServiceId} is inactive.");

        var professional = await professionalRepository.GetByIdAsync(input.ProfessionalId, cancellationToken);
        if (professional == null || professional.StoreId != input.StoreId)
            throw new NotFoundException($"Professional with ID {input.ProfessionalId} not found in store {input.StoreId}.");

        if (!professional.Active)
            throw new ConflictException($"Professional {input.ProfessionalId} is inactive.");

        if (!service.ProfessionalIds.Contains(input.ProfessionalId))
            throw new DomainValidationException($"Professional {input.ProfessionalId} is not assigned to service {input.ServiceId}.");

        var slots = await getAvailableSlotsUseCase.ExecuteAsync(
            input.StoreId,
            input.ServiceId,
            input.StartTime,
            input.ProfessionalId,
            cancellationToken);

        if (!slots.Any(s => s.ToUniversalTime() == input.StartTime.ToUniversalTime()))
        {
            throw new ConflictException("The requested time slot is no longer available.");
        }

        var endTime = input.StartTime.AddMinutes(service.DurationMinutes);
        var cancelToken = Guid.NewGuid().ToString("N");

        var customer = await customerRepository.GetByWhatsAppAsync(input.StoreId, input.CustomerWhatsApp, cancellationToken);
        if (customer == null)
        {
            customer = Customer.Create(input.StoreId, input.CustomerName, input.CustomerWhatsApp, input.CustomerEmail);
            await customerRepository.AddAsync(customer, cancellationToken);
        }

        var customerId = customer.Id;
        var customerName = customer.Name;

        var appointment = Appointment.Create(
            input.StoreId,
            input.ProfessionalId,
            input.ServiceId,
            customerName,
            input.CustomerWhatsApp,
            input.CustomerEmail,
            input.StartTime,
            endTime,
            cancelToken: cancelToken,
            customerId: customerId);

        await appointmentRepository.AddAsync(appointment, cancellationToken);

        await notificationService.SendBookingConfirmationToCustomerAsync(appointment, store, service, professional, cancellationToken);
        await notificationService.SendBookingNotificationToStoreAsync(appointment, store, service, professional, cancellationToken);

        if (professional.Email != null)
        {
            await notificationService.SendBookingNotificationToProfessionalAsync(appointment, store, service, professional, cancellationToken);
        }

        var reminderTime = input.StartTime.AddHours(-24);
        var localHour = reminderTime.Hour;

        if (localHour < 8)
        {
            reminderTime = new DateTimeOffset(
                reminderTime.Year, reminderTime.Month, reminderTime.Day,
                8, 0, 0, reminderTime.Offset);
        }
        else if (localHour >= 22)
        {
            reminderTime = new DateTimeOffset(
                reminderTime.Year, reminderTime.Month, reminderTime.Day,
                21, 0, 0, reminderTime.Offset);
        }

        if (reminderTime > timeProvider.UtcNow.AddHours(6))
        {
            backgroundJobService.ScheduleReminder(appointment.Id, reminderTime);
        }
        else
        {
            var morningReminderTime = new DateTimeOffset(
                input.StartTime.Year, input.StartTime.Month, input.StartTime.Day,
                8, 0, 0, input.StartTime.Offset);

            if (morningReminderTime > timeProvider.UtcNow.AddHours(6) &&
                morningReminderTime <= input.StartTime.AddHours(-3))
            {
                backgroundJobService.ScheduleReminder(appointment.Id, morningReminderTime);
            }
        }

        return appointment;
    }
}
