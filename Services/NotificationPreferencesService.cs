namespace bioscoop_app.Services;

/// <inheritdoc />
public class NotificationPreferencesService : INotificationPreferencesService
{
    private const string BeforeShowKey = "notify_before_show";
    private const string OnArrivalKey = "notify_on_arrival";

    public bool NotifyBeforeShow
    {
        get => Preferences.Default.Get(BeforeShowKey, false);
        set => Preferences.Default.Set(BeforeShowKey, value);
    }

    public bool NotifyOnArrival
    {
        get => Preferences.Default.Get(OnArrivalKey, false);
        set => Preferences.Default.Set(OnArrivalKey, value);
    }
}
