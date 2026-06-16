using System.Collections.ObjectModel;
using bioscoop_app.Models;
using CommunityToolkit.Mvvm.ComponentModel;

namespace bioscoop_app.ViewModels;

// Pairs a single seat with the tariff the user picked for it.
// Holds a reference to the shared tariff list so each seat shows the same dropdown options.
public partial class SeatTariffSelection : ObservableObject
{
    // When true (Tuesday screenings) the seat is shown — and totalled — at half price,
    // matching the discount the backend applies to the actual charge.
    private readonly bool _halfPrice;

    public SeatTariffSelection(SeatModel seat, ObservableCollection<TariffModel> tariffs, bool halfPrice = false)
    {
        Seat = seat;
        Tariffs = tariffs;
        _halfPrice = halfPrice;
    }

    public SeatModel Seat { get; }

    public ObservableCollection<TariffModel> Tariffs { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PriceLabel))]
    [NotifyPropertyChangedFor(nameof(EffectivePrice))]
    private TariffModel? _selectedTariff;

    // Raised whenever the chosen tariff changes, so the page total can be recomputed.
    public event Action? TariffChanged;

    // The price actually charged for this seat (half on Tuesdays). Null until a tariff is picked.
    public decimal? EffectivePrice =>
        SelectedTariff is null
            ? null
            : _halfPrice
                ? Math.Round(SelectedTariff.Price / 2m, 2, MidpointRounding.AwayFromZero)
                : SelectedTariff.Price;

    public string PriceLabel => EffectivePrice is null ? "-" : $"€ {EffectivePrice:0.00}";

    partial void OnSelectedTariffChanged(TariffModel? value) => TariffChanged?.Invoke();
}
