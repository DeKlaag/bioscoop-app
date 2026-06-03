using System.Diagnostics;
using System.Text.Json;
using bioscoop_app.Models;

namespace bioscoop_app.Services;

public class TariffService : ITariffService
{
    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _jsonOptions;
    
    public TariffService()
    {
        _http = new HttpClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
        };
    }
    
    public async Task<List<TariffModel>> GetTariffsAsync()
    {
        try
        {
            using var response = await _http.GetAsync(Constants.TariffsURL);
            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"GET {Constants.TariffsURL} -> {(int)response.StatusCode}");
                return [];
            }

            await using var stream = await response.Content.ReadAsStreamAsync();
            var tariffs = await JsonSerializer.DeserializeAsync<List<TariffModel>>(stream, _jsonOptions);
            return tariffs ?? [];
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"TariffService.GetTariffsAsync failed: {ex}");
            throw;
        }
    }
}