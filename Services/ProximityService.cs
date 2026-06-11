using System.Globalization;
using bioscoop_app.Models;
using Plugin.LocalNotification;
using Plugin.LocalNotification.Core.Models;

namespace bioscoop_app.Services;

/// <inheritdoc />
public class ProximityService : IProximityService
{
    private const double NearMeters = 500;       // "near a cinema" threshold
    private const int ProximityNotificationId = 999000001; // single, fixed id
    private const string ChannelId = "reservations";
    private const string CooldownKey = "proximity_last_alert_utc";
    private static readonly TimeSpan Cooldown = TimeSpan.FromMinutes(15);

    private readonly INotificationPreferencesService _prefs;

    public ProximityService(INotificationPreferencesService prefs) => _prefs = prefs;

    public async Task CheckAsync(IEnumerable<ReservationModel> upcoming)
    {
        if (!_prefs.NotifyOnArrival)
            return;

        var next = upcoming
            .Where(r => r.IsUpcoming && !r.IsCancelled)
            .OrderBy(r => r.StartTimeUtc)
            .FirstOrDefault();
        if (next is null)
            return;

        // Cooldown guards against repeated alerts on quick foreground/resume cycles.
        var lastRaw = Preferences.Default.Get(CooldownKey, string.Empty);
        if (DateTime.TryParse(lastRaw, CultureInfo.InvariantCulture,
                DateTimeStyles.RoundtripKind, out var last)
            && DateTime.UtcNow - last < Cooldown)
            return;

        // Reuse the existing "When in use" permission; MapPage owns the prompt UX,
        // so here we silently skip when it has not been granted.
        var status = await Permissions.CheckStatusAsync<Permissions.LocationWhenInUse>();
        if (status != PermissionStatus.Granted)
            return;

        Location? location;
        try
        {
            location = await Geolocation.GetLastKnownLocationAsync()
                       ?? await Geolocation.GetLocationAsync(
                           new GeolocationRequest(GeolocationAccuracy.Medium));
        }
        catch
        {
            return; // no GPS fix available
        }

        if (location is null)
            return;

        var near = CinemaLocations.All.Any(c =>
            Location.CalculateDistance(location.Latitude, location.Longitude,
                c.Lat, c.Lng, DistanceUnits.Kilometers) * 1000 <= NearMeters);
        if (!near)
            return;

        try
        {
            Vibration.Default.Vibrate(TimeSpan.FromMilliseconds(600));
        }
        catch
        {
            // Vibration unsupported on this device; ignore.
        }

        var granted = await LocalNotificationCenter.Current.RequestNotificationPermission();
        if (granted)
        {
            var remaining = Math.Max(0,
                (int)Math.Round((next.StartTimeLocal - DateTime.Now).TotalMinutes));
            await LocalNotificationCenter.Current.Show(new NotificationRequest
            {
                NotificationId = ProximityNotificationId,
                Title = "Je bent bij de bioscoop",
                Description = $"{next.HallDisplay} · nog {remaining} minuten tot aanvang",
                Android = { ChannelId = ChannelId },
            });
        }

        Preferences.Default.Set(CooldownKey, DateTime.UtcNow.ToString("o"));
    }
}
