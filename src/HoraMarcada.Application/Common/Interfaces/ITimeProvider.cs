namespace HoraMarcada.Application.Common.Interfaces;

public interface ITimeProvider
{
    DateTimeOffset UtcNow { get; }
}
