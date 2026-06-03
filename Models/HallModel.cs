using System.Text.Json.Serialization;

namespace bioscoop_app.Models;

public class HallModel
{
    [JsonPropertyName("hallId")]
    public Guid ID { get; set; }

    [JsonPropertyName("number")]
    public int? Number { get; set; }  

    [JsonPropertyName("layoutType")]
    public int? LayoutType { get; set; }
}