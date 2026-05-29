using System.Text.Json.Serialization;

namespace bioscoop_app.Models;

public class ScreeningModel
{
    [JsonPropertyName("screeningId")]
    public Guid ID { get; set; }

    [JsonPropertyName("startTimeUtc")]
    public DateTime StartTime { get; set; }

    [JsonPropertyName("movie")]
    public MovieModel Movie { get; set; }
    
    [JsonPropertyName("hall")]
    public HallModel Hall { get; set; }
}