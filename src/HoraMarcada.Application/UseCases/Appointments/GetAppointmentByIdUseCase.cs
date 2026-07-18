using HoraMarcada.Application.Exceptions;
using HoraMarcada.Application.Models;
using HoraMarcada.Application.Policies;
using HoraMarcada.Application.Repositories;

namespace HoraMarcada.Application.UseCases.Appointments;

public class GetAppointmentByIdUseCase(
    IAppointmentRepository appointmentRepository,
    IStoreOwnershipPolicy ownershipPolicy)
{
    public async Task<AppointmentDto> ExecuteAsync(Guid userId, Guid appointmentId, Guid storeId, CancellationToken cancellationToken = default)
    {
        await ownershipPolicy.EnsureUserOwnsStoreAsync(userId, storeId, cancellationToken);

        var appointment = await appointmentRepository.GetByIdAsync(appointmentId, cancellationToken);
        if (appointment == null || appointment.StoreId != storeId)
        {
            throw new NotFoundException($"Appointment {appointmentId} not found in store {storeId}.");
        }

        return AppointmentDto.FromDomain(appointment);
    }
}
