using System.Diagnostics;
using System.Text.Json;
using bioscoop_app.Models;

namespace bioscoop_app.Services;

// Caches the last fetched reservations locally in Preferences so the Reserveringen
// tab can paint instantly and fall back to a known list when the API is unreachable.
// Like FavoritesService this keeps things simple (a single JSON string per user) and
// skips SecureStorage — reservation data isn't sensitive. Keyed per email so multiple
// accounts on one device don't see each other's cache.
public class ReservationStore : IReservationStore
{
    private const string KeyPrefix = "reservations_cache_";

    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
    };

    public List<ReservationModel> Load(string email)
    {
        var raw = Preferences.Default.Get(KeyFor(email), string.Empty);
        if (string.IsNullOrEmpty(raw))
            return [];

        try
        {
            return JsonSerializer.Deserialize<List<ReservationModel>>(raw, JsonOptions) ?? [];
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ReservationStore.Load failed: {ex.Message}");
            return [];
        }
    }

    public void Save(string email, IReadOnlyList<ReservationModel> reservations)
    {
        var json = JsonSerializer.Serialize(reservations, JsonOptions);
        Preferences.Default.Set(KeyFor(email), json);
    }

    private static string KeyFor(string email) => KeyPrefix + email.Trim().ToLowerInvariant();
}
