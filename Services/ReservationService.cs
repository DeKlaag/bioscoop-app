using System.Diagnostics;
using System.Text.Json;
using bioscoop_app.Models;

namespace bioscoop_app.Services;

public class ReservationService : IReservationService
{
    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _jsonOptions;

    public ReservationService()
    {
        _http = new HttpClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
        };
    }

    public async Task<List<ReservationModel>> GetReservationsByEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return [];

        var url = Constants.UserReservationsUrl(email);
        try
        {
            using var response = await _http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"GET {url} -> {(int)response.StatusCode}");
                return [];
            }

            await using var stream = await response.Content.ReadAsStreamAsync();
            var reservations = await JsonSerializer.DeserializeAsync<List<ReservationModel>>(stream, _jsonOptions);
            return reservations ?? [];
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"ReservationService.GetReservationsByEmailAsync failed: {ex}");
            throw;
        }
    }
}
