using HoraMarcada.Infrastructure.Notifications.Common;
using Xunit;

namespace HoraMarcada.UnitTests.Infrastructure.Notifications;

public class AppointmentNotificationUrlsTests
{
    [Fact]
    public void BuildCancelUrl_ShouldUseShortPath()
    {
        var url = AppointmentNotificationUrls.BuildCancelUrl("https://horamarcada.com", "K7mNx2");

        Assert.Equal("https://horamarcada.com/c/K7mNx2", url);
    }

    [Fact]
    public void BuildRescheduleUrl_ShouldUseShortPath()
    {
        var url = AppointmentNotificationUrls.BuildRescheduleUrl("https://horamarcada.com", "K7mNx2");

        Assert.Equal("https://horamarcada.com/r/K7mNx2", url);
    }

    [Fact]
    public void BuildConfirmUrl_ShouldUseShortPath()
    {
        var url = AppointmentNotificationUrls.BuildConfirmUrl("https://horamarcada.com", "K7mNx2");

        Assert.Equal("https://horamarcada.com/co/K7mNx2", url);
    }
}
