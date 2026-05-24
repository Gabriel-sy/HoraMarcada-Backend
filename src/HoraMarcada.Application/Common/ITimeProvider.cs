namespace HoraMarcada.Application.Common;

public interface ITimeProvider
{
    DateTimeOffset UtcNow { get; }
}
