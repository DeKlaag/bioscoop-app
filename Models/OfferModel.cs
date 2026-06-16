using System.Text.Json.Serialization;

namespace bioscoop_app.Models;

public class OfferModel
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("title")]
    public string Title { get; set; } = string.Empty;

    [JsonPropertyName("description")]
    public string? Description { get; set; }

    // Short label shown as a badge, e.g. "Actie" or "Korting".
    [JsonPropertyName("badge")]
    public string? Badge { get; set; }

    [JsonPropertyName("imageUrl")]
    public string? ImageUrl { get; set; }

    [JsonPropertyName("validUntil")]
    public DateTime? ValidUntil { get; set; }

    [JsonIgnore]
    public string ValidUntilText =>
        ValidUntil.HasValue ? $"Geldig t/m {ValidUntil.Value:dd-MM-yyyy}" : string.Empty;

    [JsonIgnore]
    public bool HasBadge => !string.IsNullOrWhiteSpace(Badge);
}
