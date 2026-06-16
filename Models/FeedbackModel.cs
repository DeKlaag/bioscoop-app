using System.Text.Json.Serialization;

namespace bioscoop_app.Models;

public class FeedbackRequest
{
    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("rating")]
    public int Rating { get; set; }

    [JsonPropertyName("category")]
    public string? Category { get; set; }

    [JsonPropertyName("message")]
    public string Message { get; set; } = string.Empty;
}
