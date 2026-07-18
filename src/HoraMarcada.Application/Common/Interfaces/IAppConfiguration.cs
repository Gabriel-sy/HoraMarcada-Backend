namespace HoraMarcada.Application.Common.Interfaces;

public interface IAppConfiguration
{
    int MinimumSlotLeadTimeMinutes { get; }
    bool NotificationsEnabled { get; }
}
