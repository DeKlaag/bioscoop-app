using System.Globalization;
using System.Text.Json.Serialization;

namespace bioscoop_app.Models;

public class ReservationModel
{
    [JsonPropertyName("printCode")]
    public string? PrintCode { get; set; }

    [JsonPropertyName("movieTitle")]
    public string? MovieTitle { get; set; }

    [JsonPropertyName("movie")]
    public MovieModel? Movie { get; set; }

    [JsonPropertyName("hallNumber")]
    public int HallNumber { get; set; }

    [JsonPropertyName("startTimeUtc")]
    public DateTime StartTimeUtc { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("totalAmount")]
    public decimal TotalAmount { get; set; }

    [JsonPropertyName("ticketAmount")]
    public decimal TicketAmount { get; set; }

    [JsonPropertyName("arrangementAmount")]
    public decimal ArrangementAmount { get; set; }

    [JsonPropertyName("seats")]
    public List<ReservationSeatModel> Seats { get; set; } = new();

    // Local start time for display (API sends UTC).
    [JsonIgnore]
    public DateTime StartTimeLocal => StartTimeUtc.ToLocalTime();

    // Comma-separated seat labels, e.g. "A1, A2".
    [JsonIgnore]
    public string SeatsSummary => string.Join(", ", Seats.Select(s => s.Label));

    [JsonIgnore]
    public string HallDisplay => $"Zaal {HallNumber}";

    // Single subtitle line, e.g. "Vandaag · 20:15 · zaal 4".
    [JsonIgnore]
    public string WhenDisplay
    {
        get
        {
            var nl = new CultureInfo("nl-NL");
            var local = StartTimeLocal;
            var today = DateTime.Today;
            var day = local.Date == today ? "Vandaag"
                : local.Date == today.AddDays(1) ? "Morgen"
                : local.Date == today.AddDays(-1) ? "Gisteren"
                : local.ToString("ddd d MMM", nl);
            return $"{day} · {local:HH:mm} · zaal {HallNumber}";
        }
    }

    [JsonIgnore]
    public bool IsUpcoming => StartTimeUtc >= DateTime.UtcNow;
}

public class ReservationSeatModel
{
    [JsonPropertyName("orderId")]
    public Guid OrderId { get; set; }

    [JsonPropertyName("rowLabel")]
    public string? RowLabel { get; set; }

    [JsonPropertyName("seatNumber")]
    public int SeatNumber { get; set; }

    // Convenience for display, e.g. "A1"
    [JsonIgnore]
    public string Label => $"{RowLabel}{SeatNumber}";
}

