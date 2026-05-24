using HoraMarcada.Domain.Entities;

namespace HoraMarcada.Application.Repositories;

public interface IAppointmentRepository
{
    Task<Appointment?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(Appointment appointment, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Appointment>> ListByStoreAsync(
        Guid storeId,
        DateTimeOffset start,
        DateTimeOffset end,
        Guid? professionalId = null,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Appointment>> ListByProfessionalAndDateAsync(
        Guid professionalId,
        DateTimeOffset date,
        CancellationToken cancellationToken = default);

    Task<Appointment?> GetByCancelTokenAsync(
        string cancelToken,
        CancellationToken cancellationToken = default);

    Task UpdateAsync(
        Appointment appointment,
        CancellationToken cancellationToken = default);
}
