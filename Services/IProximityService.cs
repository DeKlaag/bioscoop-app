using bioscoop_app.Models;

namespace bioscoop_app.Services;

/// <summary>
/// Foreground-only proximity check: when the app comes to the foreground and the
/// user is near a Pathé cinema with an upcoming reservation, vibrate and show a
/// notification stating the hall and remaining time. No background geofencing.
/// </summary>
public interface IProximityService
{
    Task CheckAsync(IEnumerable<ReservationModel> upcoming);
}
