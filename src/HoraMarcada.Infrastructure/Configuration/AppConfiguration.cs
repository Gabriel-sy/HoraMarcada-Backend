using HoraMarcada.Application.Common.Interfaces;
using Microsoft.Extensions.Configuration;

namespace HoraMarcada.Infrastructure.Configuration;

public class AppConfiguration(IConfiguration configuration) : IAppConfiguration
{
    public int MinimumSlotLeadTimeMinutes
    {
        get
        {
            var value = configuration["MINIMUM_SLOT_LEAD_TIME_MINUTES"];
            return int.TryParse(value, out var minutes) ? minutes : 0;
        }
    }

    public bool NotificationsEnabled
    {
        get
        {
            var value = configuration["NOTIFICATIONS_ENABLED"];
            return !bool.TryParse(value, out var enabled) || enabled;
        }
    }

}
