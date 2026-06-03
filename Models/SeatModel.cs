using System.Text.Json.Serialization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace bioscoop_app.Models;

public partial class SeatModel : ObservableObject
{
    [JsonPropertyName("seatId")]
    public Guid ID { get; set; }

    [JsonPropertyName("rowLabel")]
    public string? RowLabel { get; set; }

    [JsonPropertyName("seatNumber")]
    public int SeatNumber { get; set; }

    [JsonPropertyName("isReserved")]
    public bool IsReserved { get; set; }

    // Whether the user has currently selected this seat. UI-only state.
    [JsonIgnore]
    [ObservableProperty]
    private bool _isSelected;

    // Convenience for display, e.g. "A1"
    [JsonIgnore]
    public string Label => $"{RowLabel}{SeatNumber}";

    // A seat can only be selected when it is not already reserved.
    [JsonIgnore]
    public bool IsAvailable => !IsReserved;
}
