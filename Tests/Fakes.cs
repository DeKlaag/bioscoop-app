using bioscoop_app.Models;
using bioscoop_app.Services;

namespace bioscoop_app.Tests;

// Lightweight in-memory test doubles so the service under test can be exercised
// without any backend, storage or MAUI dependency.

internal sealed class FakeMovieService : IMovieService
{
    private readonly List<MovieModel> _movies;

    public FakeMovieService(IEnumerable<MovieModel> movies) => _movies = movies.ToList();

    public Task<List<MovieModel>> GetMoviesAsync() => Task.FromResult(_movies.ToList());

    public Task<MovieModel?> GetMovieByIdAsync(Guid id) =>
        Task.FromResult(_movies.FirstOrDefault(m => m.ID == id));
}

internal sealed class FakeFavoritesService : IFavoritesService
{
    private readonly HashSet<Guid> _ids;

    public FakeFavoritesService(params Guid[] ids) => _ids = ids.ToHashSet();

    public bool IsFavorite(Guid movieId) => _ids.Contains(movieId);

    public void Toggle(Guid movieId)
    {
        if (!_ids.Add(movieId))
            _ids.Remove(movieId);
    }

    public IReadOnlyList<Guid> GetFavoriteIds() => _ids.ToList();
}
