using System.Collections.ObjectModel;
using System.Security.Claims;
using bioscoop_app.Models;
using bioscoop_app.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace bioscoop_app.ViewModels;

public partial class ReservationsViewModel : ObservableObject
{
    private readonly IReservationService _reservationService;
    private readonly IReservationStore _store;
    private readonly IUserSession _session;

    private readonly List<ReservationModel> _all = [];

    public ObservableCollection<ReservationModel> Reservations { get; } = [];

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsLoggedIn))]
    private bool _isLoggedOut;

    public bool IsLoggedIn => !IsLoggedOut;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsUpcomingSelected))]
    [NotifyPropertyChangedFor(nameof(IsHistorySelected))]
    private bool _showHistory;

    public bool IsUpcomingSelected => !ShowHistory;
    public bool IsHistorySelected => ShowHistory;

    partial void OnShowHistoryChanged(bool value) => ApplyFilter();

    [RelayCommand]
    private void ShowUpcoming() => ShowHistory = false;

    [RelayCommand]
    private void ShowPast() => ShowHistory = true;

    private void SetReservations(IEnumerable<ReservationModel> reservations)
    {
        _all.Clear();
        _all.AddRange(reservations);
        ApplyFilter();
    }

    private void ApplyFilter()
    {
        Reservations.Clear();
        var filtered = ShowHistory
            ? _all.Where(r => !r.IsUpcoming).OrderByDescending(r => r.StartTimeUtc)
            : _all.Where(r => r.IsUpcoming).OrderBy(r => r.StartTimeUtc);
        foreach (var reservation in filtered)
            Reservations.Add(reservation);
    }

    public ReservationsViewModel(IReservationService reservationService, IReservationStore store, IUserSession session)
    {
        _reservationService = reservationService;
        _store = store;
        _session = session;
    }

    [RelayCommand]
    private async Task LoadAsync()
    {
        if (IsBusy) return;
        try
        {
            IsBusy = true;
            ErrorMessage = null;

            var email = _session.User?.FindFirst("email")?.Value
                        ?? _session.User?.FindFirst(ClaimTypes.Email)?.Value;

            IsLoggedOut = string.IsNullOrWhiteSpace(email);

            if (IsLoggedOut)
            {
                _all.Clear();
                Reservations.Clear();
                return;
            }

            // Seed from the local cache first so the list paints instantly and stays
            // visible even when the API refresh below fails (offline fallback).
            SetReservations(_store.Load(email!));

            try
            {
                var reservations = await _reservationService.GetReservationsByEmailAsync(email!);
                SetReservations(reservations);
                _store.Save(email!, reservations);
            }
            catch (Exception ex)
            {
                // Keep the cached rows on screen; only surface an error if we have
                // nothing cached to show.
                if (_all.Count == 0)
                    ErrorMessage = ex.Message;
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }
}
