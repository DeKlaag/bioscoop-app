using System.Collections.ObjectModel;
using bioscoop_app.Models;
using bioscoop_app.Services;
using CommunityToolkit.Mvvm.ComponentModel;

namespace bioscoop_app.ViewModels;

[QueryProperty(nameof(Seats), "seats")]
[QueryProperty(nameof(Screening), "screening")]
public partial class PaymentViewModel : ObservableObject
{
    private readonly ITariffService _tariffService;

    public PaymentViewModel(ITariffService tariffService)
    {
        _tariffService = tariffService;
        _ = LoadTariffsAsync();
    }

    // The seats chosen on the previous (seat selection) screen.
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SeatCount))]
    private List<SeatModel>? _seats;

    // The screening these seats belong to (used for the header).
    [ObservableProperty]
    private ScreeningModel? _screening;

    [ObservableProperty]
    private string? _errorMessage;

    // The tariffs offered in each seat's dropdown.
    public ObservableCollection<TariffModel> Tariffs { get; } = [];

    // One row per chosen seat, each with its own tariff selection.
    public ObservableCollection<SeatTariffSelection> SeatSelections { get; } = [];

    public int SeatCount => Seats?.Count ?? 0;

    public string TotalPriceLabel =>
        FormatPrice(SeatSelections.Sum(s => s.SelectedTariff?.Price ?? 0m));

    private static string FormatPrice(decimal price) => $"€ {price:0.00}";

    partial void OnSeatsChanged(List<SeatModel>? value) => BuildSelections();

    private async Task LoadTariffsAsync()
    {
        try
        {
            ErrorMessage = null;
            var tariffs = await _tariffService.GetTariffsAsync();

            Tariffs.Clear();
            foreach (var tariff in tariffs.OrderBy(t => t.Sort))
                Tariffs.Add(tariff);

            BuildSelections();
        }
        catch (Exception)
        {
            ErrorMessage = "De tarieven konden niet worden geladen.";
        }
    }

    // (Re)build one selection row per seat. Runs once both the seats and the
    // tariffs are available, regardless of which arrives first.
    private void BuildSelections()
    {
        if (Seats is null || Tariffs.Count == 0) return;

        foreach (var existing in SeatSelections)
            existing.TariffChanged -= OnSelectionChanged;
        SeatSelections.Clear();

        var defaultTariff = Tariffs.FirstOrDefault();
        foreach (var seat in Seats)
        {
            var selection = new SeatTariffSelection(seat, Tariffs)
            {
                SelectedTariff = defaultTariff,
            };
            selection.TariffChanged += OnSelectionChanged;
            SeatSelections.Add(selection);
        }

        OnPropertyChanged(nameof(TotalPriceLabel));
    }

    private void OnSelectionChanged() => OnPropertyChanged(nameof(TotalPriceLabel));
}
