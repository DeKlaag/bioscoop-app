using bioscoop_app.Models;

namespace bioscoop_app.Services;

// Recommendations are computed entirely client-side: we look at the genres of the
// movies the user favorited and rank the remaining movies by how well their genre
// matches. No backend or viewing-history tracking is involved.
public class RecommendationService : IRecommendationService
{
    private readonly IMovieService _movieService;
    private readonly IFavoritesService _favoritesService;

    public RecommendationService(IMovieService movieService, IFavoritesService favoritesService)
    {
        _movieService = movieService;
        _favoritesService = favoritesService;
    }

    public async Task<List<MovieModel>> GetRecommendationsAsync(int max = 10)
    {
        var movies = await _movieService.GetMoviesAsync();
        if (movies.Count == 0)
            return [];

        var favoriteIds = _favoritesService.GetFavoriteIds().ToHashSet();

        // Genres the user has shown interest in, case-insensitive.
        var likedGenres = movies
            .Where(m => favoriteIds.Contains(m.ID) && !string.IsNullOrWhiteSpace(m.Genre))
            .Select(m => m.Genre!.Trim())
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        // Never recommend movies the user already favorited.
        var candidates = movies.Where(m => !favoriteIds.Contains(m.ID)).ToList();

        // No favorites yet: surface a general selection so the section is never empty.
        if (likedGenres.Count == 0)
            return candidates.Take(max).ToList();

        return candidates
            .OrderByDescending(m => GenreMatchScore(m, likedGenres))
            .ThenBy(m => m.Title)
            .Take(max)
            .ToList();
    }

    private static int GenreMatchScore(MovieModel movie, HashSet<string> likedGenres)
    {
        if (string.IsNullOrWhiteSpace(movie.Genre))
            return 0;
        return likedGenres.Contains(movie.Genre.Trim()) ? 1 : 0;
    }
}
