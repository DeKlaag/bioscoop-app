using bioscoop_app.Models;

namespace bioscoop_app.Services;

public interface IMovieService
{
    Task<List<MovieModel>> GetMoviesAsync();
    Task<MovieModel?> GetMovieByIdAsync(Guid id);
}