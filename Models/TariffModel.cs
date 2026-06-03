using System.Text.Json.Serialization;

namespace bioscoop_app.Models;

public class TariffModel
{
    [JsonPropertyName("tariffId")]
    public Guid TariffId { get; set; }
    
    [JsonPropertyName("tariffType")]
    public string Type { get; set; }
    
    [JsonPropertyName("displayName")]
    public string DisplayName { get; set; }
    
    [JsonPropertyName("price")]
    public decimal Price { get; set; }
    
    [JsonPropertyName("sortOrder")]
    public int Sort { get; set; }
}