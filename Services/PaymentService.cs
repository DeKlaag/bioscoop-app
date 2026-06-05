using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using bioscoop_app.Models;

namespace bioscoop_app.Services;

public class PaymentService : IPaymentService
{
    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _jsonOptions;

    public PaymentService()
    {
        _http = new HttpClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
        };
    }

    public async Task<ReservationGroupResponse?> CreateReservationAsync(
        Guid screeningId, IEnumerable<(Guid SeatId, Guid TariffId)> tickets, string? userEmail)
    {
        var request = new
        {
            ScreeningId = screeningId,
            SeatTickets = tickets.Select(t => new { t.SeatId, t.TariffId }).ToList(),
            UserEmail = userEmail,
        };

        using var response = await _http.PostAsJsonAsync(Constants.ReservationsWebsiteUrl, request, _jsonOptions);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"POST {Constants.ReservationsWebsiteUrl} -> {(int)response.StatusCode}: {body}");
            throw new HttpRequestException($"Reservering API {(int)response.StatusCode}: {body}");
        }

        return await response.Content.ReadFromJsonAsync<ReservationGroupResponse>(_jsonOptions);
    }

    public async Task<string?> CreateCheckoutUrlAsync(string printCode, Guid screeningId)
    {
        var request = new
        {
            PrintCode = printCode,
            ScreeningId = screeningId,
            FrontendBaseUrl = Constants.ApiBaseUrl,
            Flow = "website",
        };

        using var response = await _http.PostAsJsonAsync(Constants.StripeCheckoutUrl, request, _jsonOptions);
        if (!response.IsSuccessStatusCode)
        {
            var body = await response.Content.ReadAsStringAsync();
            Debug.WriteLine($"POST {Constants.StripeCheckoutUrl} -> {(int)response.StatusCode}: {body}");
            throw new HttpRequestException($"Stripe checkout API {(int)response.StatusCode}: {body}");
        }

        var result = await response.Content.ReadFromJsonAsync<CheckoutUrlResponse>(_jsonOptions);
        return result?.Url;
    }

    public async Task<string?> ConfirmAsync(string sessionId)
    {
        var request = new { SessionId = sessionId };

        try
        {
            using var response = await _http.PostAsJsonAsync(Constants.StripeConfirmUrl, request, _jsonOptions);
            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"POST {Constants.StripeConfirmUrl} -> {(int)response.StatusCode}");
                return null;
            }

            var result = await response.Content.ReadFromJsonAsync<ConfirmResponse>(_jsonOptions);
            return result?.PrintCode;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"PaymentService.ConfirmAsync failed: {ex}");
            return null;
        }
    }
}
