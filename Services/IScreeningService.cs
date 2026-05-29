using bioscoop_app.Models;

namespace bioscoop_app.Services;

public interface IScreeningService
{
    Task<List<ScreeningModel>> GetScreeningsByMovieAsync(Guid movieId);
}