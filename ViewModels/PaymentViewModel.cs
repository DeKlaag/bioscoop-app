using System.Collections.ObjectModel;
using System.Security.Claims;
using bioscoop_app.Models;
using bioscoop_app.Services;
using bioscoop_app.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace bioscoop_app.ViewModels;

[QueryProperty(nameof(Seats), "seats")]
[QueryProperty(nameof(Screening), "screening")]
public partial class PaymentViewModel : ObservableObject
{
    private readonly ITariffService _tariffService;
    private readonly IPaymentService _paymentService;
    private readonly IUserSession _session;

    public PaymentViewModel(ITariffService tariffService, IPaymentService paymentService, IUserSession session)
    {
        _tariffService = tariffService;
        _paymentService = paymentService;
        _session = session;
        _ = LoadTariffsAsync();
    }

    // The seats chosen on the previous (seat selection) screen.
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SeatCount))]
    private List<SeatModel>? _seats;

    // The screening these seats belong to (used for the header).
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HasDiscount))]
    private ScreeningModel? _screening;

    [ObservableProperty]
    private string? _errorMessage;

    // True while a reservation/checkout is being created, to disable the button.
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsNotProcessing))]
    private bool _isProcessing;

    public bool IsNotProcessing => !IsProcessing;

    // The tariffs offered in each seat's dropdown.
    public ObservableCollection<TariffModel> Tariffs { get; } = [];

    // One row per chosen seat, each with its own tariff selection.
    public ObservableCollection<SeatTariffSelection> SeatSelections { get; } = [];

    public int SeatCount => Seats?.Count ?? 0;

    public string TotalPriceLabel =>
        FormatPrice(SeatSelections.Sum(s => s.EffectivePrice ?? 0m));

    // True when the chosen screening falls on a Tuesday (cinema-local), so the
    // "half price" promotion applies. Drives the discount note on the payment screen.
    public bool HasDiscount => Screening is not null && IsHalfPriceDay(Screening);

    public string DiscountNote => "Dinsdagvoordeel: halve prijs op alle voorstellingen";

    private static string FormatPrice(decimal price) => $"€ {price:0.00}";

    // The screening start arrives as UTC; the promotion is based on the cinema's local day.
    private static bool IsHalfPriceDay(ScreeningModel screening)
    {
        var utc = screening.StartTime.Kind == DateTimeKind.Unspecified
            ? DateTime.SpecifyKind(screening.StartTime, DateTimeKind.Utc)
            : screening.StartTime.ToUniversalTime();
        return utc.ToLocalTime().DayOfWeek == DayOfWeek.Tuesday;
    }

    partial void OnSeatsChanged(List<SeatModel>? value) => BuildSelections();

    partial void OnScreeningChanged(ScreeningModel? value) => BuildSelections();

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

        var halfPrice = Screening is not null && IsHalfPriceDay(Screening);
        var defaultTariff = Tariffs.FirstOrDefault();
        foreach (var seat in Seats)
        {
            var selection = new SeatTariffSelection(seat, Tariffs, halfPrice)
            {
                SelectedTariff = defaultTariff,
            };
            selection.TariffChanged += OnSelectionChanged;
            SeatSelections.Add(selection);
        }

        OnPropertyChanged(nameof(TotalPriceLabel));
        OnPropertyChanged(nameof(HasDiscount));
    }

    private void OnSelectionChanged() => OnPropertyChanged(nameof(TotalPriceLabel));

    // Builds the one-line summary shown on the confirmation screen,
    // e.g. "Zaal 4 · 20:15 · rij C 7, 8". Seats are grouped per row.
    private string BuildScreeningSummary()
    {
        var parts = new List<string>();

        if (Screening?.Hall?.Number is int hall)
            parts.Add($"Zaal {hall}");

        if (Screening is not null)
            parts.Add($"{Screening.StartTime:HH:mm}");

        var seatGroups = SeatSelections
            .Select(s => s.Seat)
            .GroupBy(s => s.RowLabel)
            .Select(g => $"rij {g.Key} {string.Join(", ", g.OrderBy(s => s.SeatNumber).Select(s => s.SeatNumber))}");
        parts.AddRange(seatGroups);

        return string.Join(" · ", parts);
    }

    // Reserve the seats, create a Stripe Checkout session and open it in an in-app
    // WebView (CheckoutPage), which handles the payment and confirmation.
    [RelayCommand]
    private async Task ConfirmPaymentAsync()
    {
        if (IsProcessing) return;

        if (Screening is null || SeatSelections.Count == 0)
        {
            ErrorMessage = "Er zijn geen stoelen om te betalen.";
            return;
        }

        if (SeatSelections.Any(s => s.SelectedTariff is null))
        {
            ErrorMessage = "Kies voor elke stoel een tarief.";
            return;
        }

        IsProcessing = true;
        ErrorMessage = null;
        try
        {
            var tickets = SeatSelections
                .Select(s => (s.Seat.ID, s.SelectedTariff!.TariffId))
                .ToList();

            // Attach the logged-in customer's email so the reservation records who
            // made it. Null when reserving anonymously (not signed in), which is allowed.
            var userEmail = _session.User?.FindFirst("email")?.Value
                            ?? _session.User?.FindFirst(ClaimTypes.Email)?.Value;

            var reservation = await _paymentService.CreateReservationAsync(Screening.ID, tickets, userEmail);
            if (reservation is null || string.IsNullOrEmpty(reservation.PrintCode))
            {
                ErrorMessage = "De stoelen konden niet worden gereserveerd. Probeer het opnieuw.";
                return;
            }

            var checkoutUrl = await _paymentService.CreateCheckoutUrlAsync(reservation.PrintCode, Screening.ID);
            if (string.IsNullOrEmpty(checkoutUrl))
            {
                ErrorMessage = "De betaling kon niet worden gestart. Probeer het opnieuw.";
                return;
            }

            await Shell.Current.GoToAsync(nameof(CheckoutPage),
                new Dictionary<string, object>
                {
                    ["checkoutUrl"] = checkoutUrl,
                    ["printCode"] = reservation.PrintCode,
                    ["details"] = BuildScreeningSummary(),
                });
        }
        catch (HttpRequestException ex) when (
            ex.Message.Contains("already taken") || ex.Message.Contains(" 409"))
        {
            ErrorMessage = "Deze stoel(en) zijn zojuist bezet geraakt. Kies andere stoelen.";
        }
        catch (HttpRequestException)
        {
            ErrorMessage = "De betaling kon niet worden gestart. Controleer je verbinding en probeer het opnieuw.";
        }
        catch (Exception)
        {
            ErrorMessage = "Er ging iets mis tijdens het afrekenen.";
        }
        finally
        {
            IsProcessing = false;
        }
    }
}
