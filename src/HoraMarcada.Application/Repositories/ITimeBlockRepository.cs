using HoraMarcada.Domain.Entities;

namespace HoraMarcada.Application.Repositories;

public interface ITimeBlockRepository
{
    Task<TimeBlock?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task AddAsync(TimeBlock timeBlock, CancellationToken cancellationToken = default);

    Task DeleteAsync(TimeBlock timeBlock, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TimeBlock>> ListByProfessionalAndDateAsync(
        Guid professionalId,
        DateTimeOffset date,
        CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TimeBlock>> ListByProfessionalAndRangeAsync(
        Guid professionalId,
        DateTimeOffset start,
        DateTimeOffset end,
        CancellationToken cancellationToken = default);
}
