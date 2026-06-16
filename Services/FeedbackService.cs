using System.Diagnostics;
using System.Net.Http.Json;
using System.Text.Json;
using bioscoop_app.Models;

namespace bioscoop_app.Services;

public class FeedbackService : IFeedbackService
{
    private readonly HttpClient _http;
    private readonly JsonSerializerOptions _jsonOptions;

    public FeedbackService()
    {
        _http = new HttpClient();
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            PropertyNameCaseInsensitive = true,
        };
    }

    public async Task<bool> SubmitAsync(FeedbackRequest request)
    {
        try
        {
            using var response = await _http.PostAsJsonAsync(Constants.FeedbackUrl, request, _jsonOptions);
            if (!response.IsSuccessStatusCode)
            {
                Debug.WriteLine($"POST {Constants.FeedbackUrl} -> {(int)response.StatusCode}");
                return false;
            }

            return true;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"FeedbackService.SubmitAsync failed: {ex}");
            return false;
        }
    }
}
