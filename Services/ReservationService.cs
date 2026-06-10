using System.Diagnostics;
using System.Net;
using System.Net.Http.Json;
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

    public async Task<ReservationModel?> GetByCodeAsync(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return null;

        var url = Constants.ReservationByCodeUrl(code);
        using var response = await _http.GetAsync(url);
        if (!response.IsSuccessStatusCode)
        {
            Debug.WriteLine($"GET {url} -> {(int)response.StatusCode}");
            return null;
        }

        await using var stream = await response.Content.ReadAsStreamAsync();
        return await JsonSerializer.DeserializeAsync<ReservationModel>(stream, _jsonOptions);
    }

    public async Task<ReservationModel?> CancelAsync(string code)
    {
        var url = Constants.ReservationCancelUrl(code);
        using var response = await _http.PatchAsync(url, content: null);
        if (!response.IsSuccessStatusCode)
        {
            Debug.WriteLine($"PATCH {url} -> {(int)response.StatusCode}");
            return null;
        }

        await using var stream = await response.Content.ReadAsStreamAsync();
        return await JsonSerializer.DeserializeAsync<ReservationModel>(stream, _jsonOptions);
    }

    public async Task<ReservationModel?> UpdateSeatsAsync(string code, IReadOnlyList<Guid> seatIds)
    {
        var url = Constants.ReservationSeatsUrl(code);
        using var response = await _http.PutAsJsonAsync(url, new { seatIds }, _jsonOptions);

        if (response.StatusCode == HttpStatusCode.Conflict)
        {
            var msg = await response.Content.ReadAsStringAsync();
            throw new ReservationConflictException(
                string.IsNullOrWhiteSpace(msg) ? "Eén of meer stoelen zijn al bezet." : msg);
        }

        if (!response.IsSuccessStatusCode)
        {
            Debug.WriteLine($"PUT {url} -> {(int)response.StatusCode}");
            return null;
        }

        await using var stream = await response.Content.ReadAsStreamAsync();
        return await JsonSerializer.DeserializeAsync<ReservationModel>(stream, _jsonOptions);
    }

    public async Task<CheckInResultModel?> CheckInAsync(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
            return null;

        var url = Constants.ReservationCheckInUrl(code);
        using var response = await _http.PostAsync(url, content: null);
        if (!response.IsSuccessStatusCode)
        {
            Debug.WriteLine($"POST {url} -> {(int)response.StatusCode}");
            return null;
        }

        await using var stream = await response.Content.ReadAsStreamAsync();
        return await JsonSerializer.DeserializeAsync<CheckInResultModel>(stream, _jsonOptions);
    }
}
