using System.Collections.ObjectModel;
using bioscoop_app.Models;
using bioscoop_app.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace bioscoop_app.ViewModels;

// Lets the customer pick a new set of seats for an existing reservation. Seats-only:
// the new selection must contain exactly as many seats as the reservation already has.
[QueryProperty(nameof(Reservation), "reservation")]
public partial class EditSeatsViewModel : ObservableObject
{
    private readonly IScreeningService _screeningService;
    private readonly IReservationService _reservationService;

    public EditSeatsViewModel(IScreeningService screeningService, IReservationService reservationService)
    {
        _screeningService = screeningService;
        _reservationService = reservationService;
    }

    [ObservableProperty]
    private ReservationModel? _reservation;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _isBusy;

    // Seats grouped by row, plus the user's current selection.
    public ObservableCollection<SeatRow> SeatRows { get; } = new();
    public ObservableCollection<SeatModel> SelectedSeats { get; } = new();

    // The number of seats the user must end up selecting (same as the reservation's).
    public int RequiredCount => Reservation?.Seats.Count ?? 0;

    public bool CanConfirm => SelectedSeats.Count == RequiredCount && RequiredCount > 0;

    public string SelectionLabel =>
        $"{SelectedSeats.Count}/{RequiredCount} gekozen" +
        (SelectedSeats.Count == 0 ? "" : $": {string.Join(", ", SelectedSeats.Select(s => s.Label))}");

    partial void OnReservationChanged(ReservationModel? value)
    {
        if (value?.ScreeningId is { } screeningId && screeningId != Guid.Empty)
            _ = LoadSeatsAsync(screeningId);
    }

    [RelayCommand]
    private void ToggleSeat(SeatModel? seat)
    {
        if (seat is null || !seat.IsAvailable) return;

        // Don't allow selecting more than the reservation's seat count.
        if (!seat.IsSelected && SelectedSeats.Count >= RequiredCount)
            return;

        seat.IsSelected = !seat.IsSelected;

        if (seat.IsSelected)
            SelectedSeats.Add(seat);
        else
            SelectedSeats.Remove(seat);

        OnPropertyChanged(nameof(SelectionLabel));
        OnPropertyChanged(nameof(CanConfirm));
    }

    [RelayCommand]
    private async Task ConfirmAsync()
    {
        if (Reservation?.PrintCode is not { } code || !CanConfirm || IsBusy)
            return;

        ErrorMessage = null;
        try
        {
            IsBusy = true;
            var seatIds = SelectedSeats.Select(s => s.ID).ToList();
            var updated = await _reservationService.UpdateSeatsAsync(code, seatIds);
            if (updated is null)
            {
                ErrorMessage = "Stoelen wijzigen is niet gelukt. Probeer het opnieuw.";
                return;
            }

            await Shell.Current.GoToAsync("..");
        }
        catch (ReservationConflictException ex)
        {
            ErrorMessage = ex.Message;
            // The seat map is stale — reload so the now-taken seats show as occupied.
            if (Reservation?.ScreeningId is { } screeningId)
                await LoadSeatsAsync(screeningId);
        }
        catch
        {
            ErrorMessage = "Er ging iets mis. Probeer het opnieuw.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadSeatsAsync(Guid screeningId)
    {
        try
        {
            ErrorMessage = null;
            var seats = await _screeningService.GetSeatsByScreeningAsync(screeningId);

            // The seats this reservation currently holds come back as "reserved" from
            // the API (it doesn't know they're ours). Free them up so they can be
            // re-selected, and pre-select them as the starting point.
            var ownSeatIds = Reservation?.Seats
                .Where(s => s.SeatId.HasValue)
                .Select(s => s.SeatId!.Value)
                .ToHashSet() ?? new HashSet<Guid>();

            SeatRows.Clear();
            SelectedSeats.Clear();

            var rows = seats
                .OrderBy(s => s.RowLabel)
                .ThenBy(s => s.SeatNumber)
                .GroupBy(s => s.RowLabel ?? string.Empty)
                .Select(g => new SeatRow(g.Key, g))
                .ToList();

            foreach (var row in rows)
            {
                foreach (var seat in row)
                {
                    if (ownSeatIds.Contains(seat.ID))
                    {
                        seat.IsReserved = false;
                        seat.IsSelected = true;
                        SelectedSeats.Add(seat);
                    }
                }
                SeatRows.Add(row);
            }

            OnPropertyChanged(nameof(RequiredCount));
            OnPropertyChanged(nameof(SelectionLabel));
            OnPropertyChanged(nameof(CanConfirm));
        }
        catch (HttpRequestException)
        {
            ErrorMessage = "De stoelen konden niet worden geladen.";
        }
    }
}
