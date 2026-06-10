using System.Text.Json.Serialization;

namespace bioscoop_app.Models;

// Mirrors the API's CheckInResultDto returned by POST /reservations/{code}/checkin.
public class CheckInResultModel
{
    [JsonPropertyName("result")]
    public string? Result { get; set; }

    [JsonPropertyName("success")]
    public bool Success { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("checkedInAtUtc")]
    public DateTimeOffset? CheckedInAtUtc { get; set; }

    [JsonPropertyName("reservation")]
    public ReservationModel? Reservation { get; set; }
}
