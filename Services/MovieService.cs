using System.Diagnostics;
using System.Text.Json;
using bioscoop_app.Models;

namespace bioscoop_app.Services;

public class MovieService : IMovieService
{
    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _jsonOptions;

    public MovieService()
    {
        _http = new HttpClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
        };
    }

    public async Task<MovieModel?> GetMovieByIdAsync(Guid id)
    {
        var movies = await GetMoviesAsync();
        return movies.FirstOrDefault(m => m.ID == id);
    }

    public async Task<List<MovieModel>> GetMoviesAsync()
    {
        try
        {
            using var response = await _http.GetAsync(Constants.UpcomingMoviesUrl);
            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"GET {Constants.MoviesUrl} -> {(int)response.StatusCode}");
                return [];
            }

            await using var stream = await response.Content.ReadAsStreamAsync();
            var movies = await JsonSerializer.DeserializeAsync<List<MovieModel>>(stream, _jsonOptions);
            return movies ?? [];
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"MovieService.GetMoviesAsync failed: {ex}");
            throw;
        }
    }
}