using bioscoop_app.Models;
using Plugin.LocalNotification;
using Plugin.LocalNotification.Core.Models;

namespace bioscoop_app.Services;

/// <inheritdoc />
public class NotificationScheduler : INotificationScheduler
{
    // Fixed lead time: remind the user 30 minutes before the showing starts.
    private const int LeadMinutes = 30;
    private const string ChannelId = "reservations";

    private readonly INotificationPreferencesService _prefs;

    public NotificationScheduler(INotificationPreferencesService prefs) => _prefs = prefs;

    public async Task RescheduleRemindersAsync(IEnumerable<ReservationModel> reservations)
    {
        if (!_prefs.NotifyBeforeShow)
        {
            await CancelAllAsync();
            return;
        }

        var granted = await LocalNotificationCenter.Current.RequestNotificationPermission();
        if (!granted)
            return;

        // Clear-and-rebuild keeps scheduling idempotent: every refresh produces exactly
        // one reminder per upcoming reservation, never duplicates. Safe because this app
        // schedules no other notifications.
        LocalNotificationCenter.Current.CancelAll();

        var now = DateTime.Now;
        foreach (var reservation in reservations.Where(r => r.IsUpcoming && !r.IsCancelled))
        {
            var fireAt = reservation.StartTimeLocal.AddMinutes(-LeadMinutes);
            if (fireAt <= now)
                continue; // the 30-minute mark has already passed; too late to remind

            var request = new NotificationRequest
            {
                NotificationId = NotificationIdFor(reservation),
                Title = string.IsNullOrWhiteSpace(reservation.MovieTitle)
                    ? "Je voorstelling begint bijna"
                    : reservation.MovieTitle,
                Description = $"{reservation.HallDisplay} · nog {LeadMinutes} minuten tot aanvang",
                Android = { ChannelId = ChannelId },
                Schedule = new NotificationRequestSchedule { NotifyTime = fireAt },
            };

            await LocalNotificationCenter.Current.Show(request);
        }
    }

    public Task CancelAllAsync()
    {
        LocalNotificationCenter.Current.CancelAll();
        return Task.CompletedTask;
    }

    public async Task<bool> ShowTestNotificationAsync()
    {
        var granted = await LocalNotificationCenter.Current.RequestNotificationPermission();
        if (!granted)
            return false;

        await LocalNotificationCenter.Current.Show(new NotificationRequest
        {
            NotificationId = 999000002,
            Title = "Testmelding",
            Description = "Zaal 4 · nog 30 minuten tot aanvang",
            Android = { ChannelId = ChannelId },
            Schedule = new NotificationRequestSchedule
            {
                NotifyTime = DateTime.Now.AddSeconds(5),
            },
        });
        return true;
    }

    // Stable, positive 31-bit id derived from the reservation so reschedules dedupe.
    private static int NotificationIdFor(ReservationModel reservation)
    {
        var key = reservation.PrintCode ?? reservation.ScreeningId.ToString();
        return key.GetHashCode() & 0x7FFFFFFF;
    }
}
