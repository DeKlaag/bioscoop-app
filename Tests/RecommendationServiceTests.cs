using bioscoop_app.Models;
using bioscoop_app.Services;
using Xunit;

namespace bioscoop_app.Tests;

public class RecommendationServiceTests
{
    private static MovieModel Movie(string title, string? genre = null, Guid? id = null) => new()
    {
        ID = id ?? Guid.NewGuid(),
        Title = title,
        Genre = genre,
    };

    [Fact]
    public async Task GetRecommendationsAsync_WhenNoMovies_ReturnsEmptyList()
    {
        var service = new RecommendationService(
            new FakeMovieService([]),
            new FakeFavoritesService());

        var result = await service.GetRecommendationsAsync();

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetRecommendationsAsync_WhenNoFavorites_ReturnsGeneralSelectionLimitedByMax()
    {
        var movies = Enumerable.Range(1, 5).Select(i => Movie($"Film {i}", "Drama")).ToList();
        var service = new RecommendationService(
            new FakeMovieService(movies),
            new FakeFavoritesService()); // no favorites yet

        var result = await service.GetRecommendationsAsync(max: 3);

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task GetRecommendationsAsync_NeverRecommendsAlreadyFavoritedMovies()
    {
        var favorite = Movie("Inception", "Sci-Fi");
        var other = Movie("Interstellar", "Sci-Fi");
        var service = new RecommendationService(
            new FakeMovieService([favorite, other]),
            new FakeFavoritesService(favorite.ID));

        var result = await service.GetRecommendationsAsync();

        Assert.DoesNotContain(result, m => m.ID == favorite.ID);
        Assert.Contains(result, m => m.ID == other.ID);
    }

    [Fact]
    public async Task GetRecommendationsAsync_RanksMoviesMatchingLikedGenresFirst()
    {
        var favorite = Movie("Liked Sci-Fi", "Sci-Fi");
        var matching = Movie("Another Sci-Fi", "Sci-Fi");
        var nonMatching = Movie("A Comedy", "Comedy");

        var service = new RecommendationService(
            new FakeMovieService([favorite, nonMatching, matching]),
            new FakeFavoritesService(favorite.ID));

        var result = await service.GetRecommendationsAsync();

        // The Sci-Fi candidate must outrank the Comedy candidate.
        Assert.Equal(matching.ID, result[0].ID);
        Assert.Equal(nonMatching.ID, result[1].ID);
    }

    [Fact]
    public async Task GetRecommendationsAsync_MatchesGenresCaseInsensitivelyAndTrimmed()
    {
        var favorite = Movie("Liked", "  Horror ");
        var matching = Movie("Spooky", "horror");
        var nonMatching = Movie("Funny", "Comedy");

        var service = new RecommendationService(
            new FakeMovieService([favorite, nonMatching, matching]),
            new FakeFavoritesService(favorite.ID));

        var result = await service.GetRecommendationsAsync();

        Assert.Equal(matching.ID, result[0].ID);
    }

    [Fact]
    public async Task GetRecommendationsAsync_RespectsMaxWhenRanking()
    {
        var favorite = Movie("Liked", "Action");
        var candidates = Enumerable.Range(1, 10).Select(i => Movie($"Action {i}", "Action")).ToList();

        var service = new RecommendationService(
            new FakeMovieService(candidates.Prepend(favorite)),
            new FakeFavoritesService(favorite.ID));

        var result = await service.GetRecommendationsAsync(max: 4);

        Assert.Equal(4, result.Count);
    }
}
