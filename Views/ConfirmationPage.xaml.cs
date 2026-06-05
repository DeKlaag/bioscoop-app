namespace bioscoop_app.Views;

// Final screen of the booking flow. Shown after a successful payment instead of a
// plain alert: a success icon plus a card with the reservation number and screening
// details (hall · time · seats).
[QueryProperty(nameof(Code), "code")]
[QueryProperty(nameof(Details), "details")]
public partial class ConfirmationPage : ContentPage
{
    public ConfirmationPage()
    {
        InitializeComponent();
        BindingContext = this;
    }

    private string? _code;

    // The reservation / pickup code shown under "Reserveringsnr.".
    public string? Code
    {
        get => _code;
        set { _code = value; OnPropertyChanged(); }
    }

    private string? _details;

    // Human-readable screening summary, e.g. "Zaal 4 · 20:15 · rij C 7-8".
    public string? Details
    {
        get => _details;
        set { _details = value; OnPropertyChanged(); }
    }

    private async void OnDoneClicked(object? sender, EventArgs e)
    {
        // Booking flow is complete; return to the home tab.
        await Shell.Current.GoToAsync("//MainPage");
    }
}
