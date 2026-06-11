using bioscoop_app.Models;

namespace bioscoop_app.Services;

/// <summary>
/// Schedules local "your showing starts soon" reminders for upcoming reservations.
/// </summary>
public interface INotificationScheduler
{
    /// <summary>
    /// Cancels any previously scheduled reminders and (re)schedules a reminder
    /// 30 minutes before the start of every upcoming, non-cancelled reservation.
    /// No-op when the user has disabled the before-show preference.
    /// </summary>
    Task RescheduleRemindersAsync(IEnumerable<ReservationModel> reservations);

    /// <summary>Cancels all scheduled reminders.</summary>
    Task CancelAllAsync();

    /// <summary>
    /// Requests notification permission and fires a sample reminder a few seconds
    /// from now, so the user can verify notifications work on their device.
    /// Returns false when permission was denied.
    /// </summary>
    Task<bool> ShowTestNotificationAsync();
}
