namespace bioscoop_app.Services;

public interface IFavoritesService
{
    bool IsFavorite(Guid movieId);
    void Toggle(Guid movieId);
    IReadOnlyList<Guid> GetFavoriteIds();
}

// Stores favorite movie IDs locally in Preferences. Favorites aren't sensitive,
// so unlike UserSession this skips SecureStorage and keeps things simple: one
// comma-separated string under a single key.
public class FavoritesService : IFavoritesService
{
    private const string FavoritesKey = "favorite_movie_ids";

    public bool IsFavorite(Guid movieId) => Load().Contains(movieId);

    public void Toggle(Guid movieId)
    {
        var ids = Load();
        if (!ids.Add(movieId))
            ids.Remove(movieId);
        Save(ids);
    }

    public IReadOnlyList<Guid> GetFavoriteIds() => Load().ToList();

    private static HashSet<Guid> Load()
    {
        var raw = Preferences.Default.Get(FavoritesKey, string.Empty);
        if (string.IsNullOrEmpty(raw))
            return [];

        return raw
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(s => Guid.TryParse(s, out var id) ? id : (Guid?)null)
            .Where(id => id.HasValue)
            .Select(id => id!.Value)
            .ToHashSet();
    }

    private static void Save(HashSet<Guid> ids)
    {
        Preferences.Default.Set(FavoritesKey, string.Join(',', ids));
    }
}
