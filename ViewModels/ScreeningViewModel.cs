using System.Collections.ObjectModel;
using bioscoop_app.Models;
using bioscoop_app.Services;
using bioscoop_app.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace bioscoop_app.ViewModels;

[QueryProperty(nameof(Screening), "screening")]
public partial class ScreeningViewModel : ObservableObject
{
    private readonly IScreeningService _screeningService;

    public ScreeningViewModel(IScreeningService screeningService)
        => _screeningService = screeningService;

    [ObservableProperty]
    private ScreeningModel? _screening;

    [ObservableProperty]
    private string? _errorMessage;

    // Seats grouped by row label, ordered by row then seat number.
    public ObservableCollection<SeatRow> SeatRows { get; } = new();

    // The seats the user has currently selected, in selection order.
    public ObservableCollection<SeatModel> SelectedSeats { get; } = new();

    // Comma-separated list of selected seat labels, e.g. "A4, A5".
    public string SelectedSeatsLabel =>
        SelectedSeats.Count == 0
            ? "Nog geen stoel gekozen"
            : string.Join(", ", SelectedSeats.Select(s => s.Label));

    partial void OnScreeningChanged(ScreeningModel? value)
    {
        if (value is null) return;
        _ = LoadSeatsAsync(value.ID);
    }

    // Toggle a seat in/out of the selection. Multiple seats can be selected.
    [RelayCommand]
    private void ToggleSeat(SeatModel? seat)
    {
        if (seat is null || !seat.IsAvailable) return;

        seat.IsSelected = !seat.IsSelected;

        if (seat.IsSelected)
            SelectedSeats.Add(seat);
        else
            SelectedSeats.Remove(seat);

        OnPropertyChanged(nameof(SelectedSeatsLabel));
    }

    // Navigate to the payment screen, carrying the chosen seats and screening.
    [RelayCommand]
    private async Task ProceedToPaymentAsync()
    {
        if (SelectedSeats.Count == 0)
        {
            ErrorMessage = "Kies eerst een stoel.";
            return;
        }

        ErrorMessage = null;
        await Shell.Current.GoToAsync(nameof(PaymentPage),
            new Dictionary<string, object>
            {
                ["seats"] = SelectedSeats.ToList(),
                ["screening"] = Screening!,
            });
    }

    private async Task LoadSeatsAsync(Guid screeningId)
    {
        try
        {
            ErrorMessage = null;
            var seats = await _screeningService.GetSeatsByScreeningAsync(screeningId);

            var rows = seats
                .OrderBy(s => s.RowLabel)
                .ThenBy(s => s.SeatNumber)
                .GroupBy(s => s.RowLabel ?? string.Empty)
                .Select(g => new SeatRow(g.Key, g))
                .ToList();

            SeatRows.Clear();
            SelectedSeats.Clear();
            foreach (var row in rows) SeatRows.Add(row);

            SuggestBestSeat(rows);
            OnPropertyChanged(nameof(SelectedSeatsLabel));
        }
        catch (HttpRequestException)
        {
            ErrorMessage = "De stoelen konden niet worden geladen.";
        }
    }

    // Pre-select the "best" available seat: as central as possible, in a row
    // about two-thirds towards the back of the hall (a common ideal viewing spot).
    private void SuggestBestSeat(IReadOnlyList<SeatRow> rows)
    {
        if (rows.Count == 0) return;

        // Rows are ordered front (index 0) to back. Aim for ~60% towards the back.
        double idealRowIndex = (rows.Count - 1) * 0.6;

        SeatModel? best = null;
        double bestScore = double.MaxValue;

        for (int r = 0; r < rows.Count; r++)
        {
            var row = rows[r];
            if (row.Count == 0) continue;

            double rowCenter = (row.Min(s => s.SeatNumber) + row.Max(s => s.SeatNumber)) / 2.0;

            foreach (var seat in row)
            {
                if (!seat.IsAvailable) continue;

                // Lower score = closer to the ideal row and the centre of that row.
                double rowDistance = Math.Abs(r - idealRowIndex);
                double seatDistance = Math.Abs(seat.SeatNumber - rowCenter);
                double score = (rowDistance * 2) + seatDistance;

                if (score < bestScore)
                {
                    bestScore = score;
                    best = seat;
                }
            }
        }

        if (best is not null)
            ToggleSeat(best);
    }
}
