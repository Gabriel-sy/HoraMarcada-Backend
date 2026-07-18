using HoraMarcada.Application.Common.Interfaces;

namespace HoraMarcada.Application.Common.Services;

public class SystemTimeProvider : ITimeProvider
{
    public DateTimeOffset UtcNow => DateTimeOffset.UtcNow;
}
