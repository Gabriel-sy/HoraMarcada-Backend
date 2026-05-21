namespace Marque.Application.Common;

public interface ITimeProvider
{
    DateTimeOffset UtcNow { get; }
}
