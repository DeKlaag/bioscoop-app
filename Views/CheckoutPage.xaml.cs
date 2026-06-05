using bioscoop_app.Services;

namespace bioscoop_app.Views;

// Hosts the Stripe Checkout page in an in-app WebView. Because Stripe's success/cancel
// redirects are plain web URLs (not mobile deep links), we watch every navigation:
//   - the success URL carries ?session_id=... -> confirm the payment, then show the ticket.
//   - the cancel URL routes back to /SelectTickets/ -> treat as cancelled.
[QueryProperty(nameof(CheckoutUrl), "checkoutUrl")]
[QueryProperty(nameof(PrintCode), "printCode")]
[QueryProperty(nameof(Details), "details")]
public partial class CheckoutPage : ContentPage
{
    private readonly IPaymentService _paymentService;
    private bool _handled;

    public CheckoutPage(IPaymentService paymentService)
    {
        InitializeComponent();
        _paymentService = paymentService;
    }

    public string? PrintCode { get; set; }

    // Screening summary forwarded to the confirmation screen ("Zaal 4 · 20:15 · rij C 7-8").
    public string? Details { get; set; }

    private string? _checkoutUrl;
    public string? CheckoutUrl
    {
        get => _checkoutUrl;
        set
        {
            _checkoutUrl = value;
            if (!string.IsNullOrEmpty(value))
                Web.Source = value;
        }
    }

    private async void OnWebViewNavigating(object? sender, WebNavigatingEventArgs e)
    {
        if (_handled) return;

        var url = e.Url ?? string.Empty;

        // Successful payment: Stripe redirects to the success_url with a session id.
        if (url.Contains("session_id=", StringComparison.OrdinalIgnoreCase))
        {
            _handled = true;
            e.Cancel = true; // don't bother loading the success page itself
            await HandleSuccessAsync(url);
            return;
        }

        // Cancelled / back: Stripe redirects to the cancel_url (.../SelectTickets/{id}).
        if (url.Contains("/SelectTickets/", StringComparison.OrdinalIgnoreCase))
        {
            _handled = true;
            e.Cancel = true;
            await DisplayAlertAsync("Betaling geannuleerd", "De betaling is geannuleerd.", "OK");
            await Shell.Current.GoToAsync("..");
        }
    }

    private async Task HandleSuccessAsync(string url)
    {
        BusyOverlay.IsVisible = true;
        try
        {
            var sessionId = ExtractQueryValue(url, "session_id");
            string? confirmedCode = null;
            if (!string.IsNullOrEmpty(sessionId))
                confirmedCode = await _paymentService.ConfirmAsync(sessionId);

            // Use the confirmed pickup code when available, otherwise fall back to the
            // reservation's print code so the user still has a reference.
            var code = !string.IsNullOrEmpty(confirmedCode) ? confirmedCode : PrintCode;

            BusyOverlay.IsVisible = false;

            // Replace the checkout WebView with the confirmation screen.
            await Shell.Current.GoToAsync(nameof(ConfirmationPage),
                new Dictionary<string, object>
                {
                    ["code"] = code ?? "-",
                    ["details"] = Details ?? string.Empty,
                });
        }
        finally
        {
            BusyOverlay.IsVisible = false;
        }
    }

    private static string? ExtractQueryValue(string url, string key)
    {
        var query = new Uri(url).Query;
        foreach (var pair in query.TrimStart('?').Split('&', StringSplitOptions.RemoveEmptyEntries))
        {
            var parts = pair.Split('=', 2);
            if (parts.Length == 2 && string.Equals(parts[0], key, StringComparison.OrdinalIgnoreCase))
                return Uri.UnescapeDataString(parts[1]);
        }
        return null;
    }
}
