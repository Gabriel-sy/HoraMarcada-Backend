namespace HoraMarcada.Application.Common;

public interface IAppConfiguration
{
    int MinimumSlotLeadTimeMinutes { get; }
    bool NotificationsEnabled { get; }
}
