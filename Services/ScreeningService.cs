using System.Diagnostics;
using System.Text.Json;
using bioscoop_app.Models;

namespace bioscoop_app.Services;

public class ScreeningService : IScreeningService
{
    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _jsonOptions;

    public ScreeningService()
    {
        _http = new HttpClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
        };
    }

    public async Task<List<ScreeningModel>> GetScreeningsByMovieAsync(Guid movieId)
    {
        var url = $"{Constants.ScreeningsURL}?movieId={movieId}";
        try
        {
            using var response = await _http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"GET {url} -> {(int)response.StatusCode}");
                return [];
            }

            await using var stream = await response.Content.ReadAsStreamAsync();
            var screenings = await JsonSerializer.DeserializeAsync<List<ScreeningModel>>(stream, _jsonOptions);
            return screenings ?? [];
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ScreeningService.GetScreeningsByMovieAsync failed: {ex}");
            throw;
        }
    }

    public async Task<List<SeatModel>> GetSeatsByScreeningAsync(Guid screeningId)
    {
        var url = Constants.ScreeningSeatsUrl(screeningId);
        try
        {
            using var response = await _http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"GET {url} -> {(int)response.StatusCode}");
                return [];
            }

            await using var stream = await response.Content.ReadAsStreamAsync();
            var seats = await JsonSerializer.DeserializeAsync<List<SeatModel>>(stream, _jsonOptions);
            return seats ?? [];
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ScreeningService.GetSeatsByScreeningAsync failed: {ex}");
            throw;
        }
    }
}
