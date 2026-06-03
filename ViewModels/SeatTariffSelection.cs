using System.Collections.ObjectModel;
using bioscoop_app.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace bioscoop_app.ViewModels;

// Pairs a single seat with the tariff the user picked for it.
// Holds a reference to the shared tariff list so each seat shows the same dropdown options.
public partial class SeatTariffSelection : ObservableObject
{
    public SeatTariffSelection(SeatModel seat, ObservableCollection<TariffModel> tariffs)
    {
        Seat = seat;
        Tariffs = tariffs;
    }

    public SeatModel Seat { get; }

    public ObservableCollection<TariffModel> Tariffs { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PriceLabel))]
    private TariffModel? _selectedTariff;

    // Raised whenever the chosen tariff changes, so the page total can be recomputed.
    public event Action? TariffChanged;

    public string PriceLabel => SelectedTariff is null ? "-" : $"€ {SelectedTariff.Price:0.00}";

    partial void OnSelectedTariffChanged(TariffModel? value) => TariffChanged?.Invoke();
}
