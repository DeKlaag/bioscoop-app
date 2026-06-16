using bioscoop_app.Models;

namespace bioscoop_app.Services;

public interface IRecommendationService
{
    /// <summary>
    /// Returns movies recommended for the user, ranked by genre overlap with their
    /// favorites. Falls back to a general selection when there are no favorites yet.
    /// </summary>
    Task<List<MovieModel>> GetRecommendationsAsync(int max = 10);
}
