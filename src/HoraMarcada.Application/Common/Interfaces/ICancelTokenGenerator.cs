using HoraMarcada.Application.Repositories;

namespace HoraMarcada.Application.Common.Interfaces;

public interface ICancelTokenGenerator
{
    string Generate(int length = 6);

    Task<string> GenerateUniqueAsync(
        IAppointmentRepository appointmentRepository,
        CancellationToken cancellationToken = default);
}
