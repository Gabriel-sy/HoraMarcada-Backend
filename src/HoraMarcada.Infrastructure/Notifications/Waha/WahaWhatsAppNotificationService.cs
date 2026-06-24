using HoraMarcada.Domain.Entities;
using HoraMarcada.Domain.ValueObjects;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HoraMarcada.Infrastructure.Notifications.Waha;

public class WahaWhatsAppNotificationService(
    IWahaClient wahaClient,
    IWhatsAppHumanBehavior humanBehavior,
    IConfiguration configuration,
    ILogger<WahaWhatsAppNotificationService> logger) : INotificationDispatcher
{
    private readonly string _frontendUrl = (configuration["FrontendUrl"] ?? "https://horamarcada.site").TrimEnd('/');

    public Task SendBookingConfirmationToCustomerAsync(
        Appointment appointment,
        Store store,
        Service service,
        Professional professional,
        CancellationToken cancellationToken = default)
    {
        var text = WhatsAppMessageTemplates.BookingConfirmationToCustomer(
            appointment,
            store,
            service,
            professional,
            BuildCancelUrl(appointment));

        return SendToPhoneAsync(appointment.CustomerWhatsApp, text, ResolveLocation(store), cancellationToken);
    }

    public Task SendBookingNotificationToProfessionalAsync(
        Appointment appointment,
        Store store,
        Service service,
        Professional professional,
        CancellationToken cancellationToken = default)
    {
        if (professional.WhatsApp is null)
        {
            return Task.CompletedTask;
        }

        var text = WhatsAppMessageTemplates.BookingNotificationToProfessional(
            appointment,
            service,
            professional,
            BuildCancelUrl(appointment));

        return SendToPhoneAsync(professional.WhatsApp, text, cancellationToken: cancellationToken);
    }

    public Task SendCancellationNoticeToCustomerAsync(
        Appointment appointment,
        Store store,
        Service service,
        Professional professional,
        CancellationToken cancellationToken = default)
    {
        var text = WhatsAppMessageTemplates.CancellationNoticeToCustomer(
            appointment,
            service,
            professional,
            BuildStoreUrl(store));

        return SendToPhoneAsync(appointment.CustomerWhatsApp, text, cancellationToken: cancellationToken);
    }

    public Task SendCancellationNotificationToProfessionalAsync(
        Appointment appointment,
        Store store,
        Service service,
        Professional professional,
        CancellationToken cancellationToken = default)
    {
        if (professional.WhatsApp is null)
        {
            return Task.CompletedTask;
        }

        var text = WhatsAppMessageTemplates.CancellationNotificationToProfessional(
            appointment,
            service,
            professional.Name);

        return SendToPhoneAsync(professional.WhatsApp, text, cancellationToken: cancellationToken);
    }

    public Task SendBookingReminder24hAsync(
        Appointment appointment,
        Store store,
        Service service,
        Professional professional,
        CancellationToken cancellationToken = default)
    {
        var text = WhatsAppMessageTemplates.BookingReminder24h(
            appointment,
            service,
            professional,
            BuildCancelUrl(appointment),
            BuildRescheduleUrl(appointment));

        return SendToPhoneAsync(appointment.CustomerWhatsApp, text, ResolveLocation(store), cancellationToken);
    }

    private async Task SendToPhoneAsync(
        PhoneNumber phone,
        string text,
        WhatsAppLocationShare? location = null,
        CancellationToken cancellationToken = default)
    {
        var sessionStatus = await wahaClient.GetSessionStatusAsync(cancellationToken);
        if (sessionStatus != WahaSessionStatus.Working)
        {
            logger.LogError("WAHA session is not WORKING (status: {Status}). Message not sent.", sessionStatus);
            throw new NotificationNonRetryableException($"WAHA session is not WORKING (status: {sessionStatus}).");
        }

        var phoneDigits = WhatsAppChatIdFormatter.ToPhoneDigits(phone);
        var checkResult = await wahaClient.CheckContactExistsAsync(phoneDigits, cancellationToken);

        if (!checkResult.NumberExists || string.IsNullOrWhiteSpace(checkResult.ChatId))
        {
            logger.LogWarning("WhatsApp number {Phone} not found. Skipping message.", phone.Value);
            return;
        }

        await humanBehavior.WaitInterMessageDelayAsync(cancellationToken);
        await humanBehavior.SimulateTypingBeforeSendAsync(wahaClient, checkResult.ChatId, text, cancellationToken);
        await wahaClient.SendTextAsync(checkResult.ChatId, text, cancellationToken);

        if (location is not null)
        {
            await humanBehavior.WaitFollowUpDelayAsync(cancellationToken);
            await humanBehavior.SimulateShortTypingBeforeSendAsync(wahaClient, checkResult.ChatId, cancellationToken);
            await wahaClient.SendLocationAsync(
                checkResult.ChatId,
                location.Latitude,
                location.Longitude,
                cancellationToken);
        }

        humanBehavior.NotifyMessageSent();
    }

    private string BuildCancelUrl(Appointment appointment)
    {
        return AppointmentNotificationUrls.BuildCancelUrl(_frontendUrl, appointment.CancelToken!);
    }

    private string BuildStoreUrl(Store store)
    {
        return $"{_frontendUrl}/{store.Slug.Value}";
    }

    private string BuildRescheduleUrl(Appointment appointment)
    {
        return AppointmentNotificationUrls.BuildRescheduleUrl(_frontendUrl, appointment.CancelToken!);
    }

    private static WhatsAppLocationShare? ResolveLocation(Store store)
    {
        if (store.AddressLat is null || store.AddressLng is null)
        {
            return null;
        }

        return new WhatsAppLocationShare(
            (double)store.AddressLat.Value,
            (double)store.AddressLng.Value);
    }
}
