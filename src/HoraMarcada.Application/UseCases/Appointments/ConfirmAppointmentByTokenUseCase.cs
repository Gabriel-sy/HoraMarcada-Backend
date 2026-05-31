using HoraMarcada.Application.Common;
using HoraMarcada.Application.Exceptions;
using HoraMarcada.Application.Repositories;
using HoraMarcada.Domain.Exceptions;

namespace HoraMarcada.Application.UseCases.Appointments;

public class ConfirmAppointmentByTokenUseCase(
    IAppointmentRepository appointmentRepository)
{
    public async Task ExecuteAsync(string cancelToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(cancelToken))
            throw new DomainValidationException("Cancel token is required.");

        var appointment = await appointmentRepository.GetByCancelTokenAsync(cancelToken, cancellationToken);
        if (appointment == null)
        {
            throw new NotFoundException("Appointment with the provided cancel token not found.");
        }

        if (appointment.CancelTokenUsed)
        {
            throw new ConflictException("The cancel token has already been used.");
        }

        appointment.ConfirmPresence();

        await appointmentRepository.UpdateAsync(appointment, cancellationToken);
    }
}
