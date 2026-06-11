namespace bioscoop_app.Services;

/// <summary>
/// User-controlled toggles for whether the app may send notifications.
/// Backed by <see cref="Microsoft.Maui.Storage.Preferences"/>, defaulting to off
/// until the user explicitly opts in.
/// </summary>
public interface INotificationPreferencesService
{
    /// <summary>Send a local reminder 30 minutes before an upcoming showing.</summary>
    bool NotifyBeforeShow { get; set; }

    /// <summary>Vibrate and notify when the user is near a cinema with an upcoming showing.</summary>
    bool NotifyOnArrival { get; set; }
}
