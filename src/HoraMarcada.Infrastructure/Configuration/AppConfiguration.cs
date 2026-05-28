using HoraMarcada.Application.Common;
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
}
