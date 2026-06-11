using System.Security.Claims;
using bioscoop_app.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace bioscoop_app.ViewModels;

public partial class NotificationPreferencesViewModel : ObservableObject
{
    private readonly INotificationPreferencesService _prefs;
    private readonly INotificationScheduler _scheduler;
    private readonly IReservationStore _store;
    private readonly IUserSession _session;

    [ObservableProperty]
    private bool _notifyBeforeShow;

    [ObservableProperty]
    private bool _notifyOnArrival;

    [ObservableProperty]
    private string? _statusMessage;

    public NotificationPreferencesViewModel(
        INotificationPreferencesService prefs,
        INotificationScheduler scheduler,
        IReservationStore store,
        IUserSession session)
    {
        _prefs = prefs;
        _scheduler = scheduler;
        _store = store;
        _session = session;

        _notifyBeforeShow = _prefs.NotifyBeforeShow;
        _notifyOnArrival = _prefs.NotifyOnArrival;
    }

    partial void OnNotifyBeforeShowChanged(bool value)
    {
        _prefs.NotifyBeforeShow = value;
        _ = ApplyReminderChangeAsync();
    }

    partial void OnNotifyOnArrivalChanged(bool value)
    {
        _prefs.NotifyOnArrival = value;
    }

    [RelayCommand]
    private async Task SendTestNotificationAsync()
    {
        var granted = await _scheduler.ShowTestNotificationAsync();
        StatusMessage = granted
            ? "Testmelding verstuurd — verschijnt over ~5 seconden. Vergrendel je telefoon om de melding te zien."
            : "Meldingen zijn uitgeschakeld voor deze app. Zet ze aan via iOS-instellingen → bioscoop-app → Berichtgeving.";
    }

    private async Task ApplyReminderChangeAsync()
    {
        try
        {
            if (!_prefs.NotifyBeforeShow)
            {
                await _scheduler.CancelAllAsync();
                return;
            }

            var email = _session.User?.FindFirst("email")?.Value
                        ?? _session.User?.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrWhiteSpace(email))
                return;

            // Use the cached reservations so toggling never triggers a network call.
            await _scheduler.RescheduleRemindersAsync(_store.Load(email));
        }
        catch
        {
            // Best-effort; a failed (re)schedule must not crash the toggle.
        }
    }
}
