using Marque.Application.Exceptions;
using Marque.Application.Repositories;
using Marque.Domain.Exceptions;

namespace Marque.Application.UseCases.Appointments;

public class CancelAppointmentByTokenUseCase(IAppointmentRepository appointmentRepository)
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

        appointment.Cancel();
        appointment.MarkCancelTokenUsed();

        await appointmentRepository.UpdateAsync(appointment, cancellationToken);
    }
}
