using System.Text.Json.Serialization;

namespace bioscoop_app.Models;

public class MovieModel
{
    [JsonPropertyName("movieId")]
    public Guid ID { get; set; }
    public string Title {get; set;}
    public string? ImageUrl { get; set; }
    public string? Description { get; set; }
    public int? DurationMinutes { get; set; }
    public string? Genre { get; set; }
    public int? Age { get; set; }
}