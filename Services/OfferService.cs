using System.Diagnostics;
using System.Text.Json;
using bioscoop_app.Models;

namespace bioscoop_app.Services;

// Offers are bundled with the app as a raw asset (Resources/Raw/offers.json) rather
// than fetched from the backend. Reads the packaged file and deserializes it, mirroring
// the graceful-degradation style of the HTTP services (return [] on any failure).
public class OfferService : IOfferService
{
    private const string OffersFileName = "offers.json";
    private readonly JsonSerializerOptions _jsonOptions;

    public OfferService()
    {
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
        };
    }

    public async Task<List<OfferModel>> GetOffersAsync()
    {
        try
        {
            await using var stream = await FileSystem.OpenAppPackageFileAsync(OffersFileName);
            var offers = await JsonSerializer.DeserializeAsync<List<OfferModel>>(stream, _jsonOptions);
            return offers ?? [];
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"OfferService.GetOffersAsync failed: {ex}");
            return [];
        }
    }
}
