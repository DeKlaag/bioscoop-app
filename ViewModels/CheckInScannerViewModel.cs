using bioscoop_app.Models;
using bioscoop_app.Services;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace bioscoop_app.ViewModels;

// Drives the staff QR check-in scanner: a scanned reservation QR is resolved to a
// print code and sent to the API, then the outcome is shown as a coloured result card.
public partial class CheckInScannerViewModel : ObservableObject
{
    private readonly IReservationService _reservationService;

    public CheckInScannerViewModel(IReservationService reservationService)
        => _reservationService = reservationService;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsScanning))]
    [NotifyPropertyChangedFor(nameof(IsPaused))]
    private bool _isProcessing;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsScanning))]
    [NotifyPropertyChangedFor(nameof(IsPaused))]
    private bool _hasResult;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsScanning))]
    private bool _cameraAuthorized;

    [ObservableProperty]
    private string? _resultTitle;

    [ObservableProperty]
    private string? _resultDetail;

    [ObservableProperty]
    private Color _resultColor = Colors.Gray;

    // True while the camera is live and ready to read (used for the on-screen hint).
    public bool IsScanning => CameraAuthorized && !IsProcessing && !HasResult;

    // Pause detection (camera stays on) while processing a scan or showing a result.
    // Bound to CameraView.PauseScanning.
    public bool IsPaused => IsProcessing || HasResult;

    private string? _lastCode;

    // Called by the page when the camera detects a barcode.
    public async Task HandleScanAsync(string? raw)
    {
        if (IsProcessing || HasResult || string.IsNullOrWhiteSpace(raw))
            return;

        var code = ExtractCode(raw);
        if (string.IsNullOrWhiteSpace(code) || code == _lastCode)
            return;

        _lastCode = code;
        IsProcessing = true;
        try
        {
            var result = await _reservationService.CheckInAsync(code);
            ShowResult(result);
        }
        catch
        {
            ResultTitle = "Fout";
            ResultDetail = "Kon niet inchecken. Controleer de verbinding.";
            ResultColor = Colors.OrangeRed;
        }
        finally
        {
            IsProcessing = false;
            HasResult = true;
        }
    }

    [RelayCommand]
    private void ScanNext()
    {
        // Allow the same ticket to be re-scanned after the staff dismisses the card.
        _lastCode = null;
        ResultTitle = null;
        ResultDetail = null;
        ResultColor = Colors.Gray;
        HasResult = false;
    }

    private void ShowResult(CheckInResultModel? result)
    {
        if (result is null)
        {
            ResultTitle = "Onbekende code";
            ResultDetail = "Deze QR-code hoort niet bij een reservering.";
            ResultColor = Colors.OrangeRed;
            return;
        }

        var movie = result.Reservation?.MovieTitle;
        var seats = result.Reservation?.SeatsSummary;
        var detail = string.IsNullOrWhiteSpace(movie)
            ? result.Message
            : $"{movie}\n{result.Reservation?.HallDisplay} · {seats}\n{result.Message}";

        ResultDetail = detail;
        (ResultTitle, ResultColor) = result.Result switch
        {
            CheckInResultNames.CheckedIn        => ("Ingecheckt ✓", Colors.SeaGreen),
            CheckInResultNames.AlreadyCheckedIn => ("Al ingecheckt", Colors.Goldenrod),
            CheckInResultNames.NotPaid          => ("Niet betaald", Colors.OrangeRed),
            CheckInResultNames.Cancelled        => ("Geannuleerd", Colors.OrangeRed),
            CheckInResultNames.TooEarly         => ("Te vroeg", Colors.Goldenrod),
            CheckInResultNames.Expired          => ("Verlopen", Colors.OrangeRed),
            _                                   => ("Niet gevonden", Colors.OrangeRed),
        };
    }

    // The QR encodes the by-code lookup URL; pull the print code off the end. Falls
    // back to the raw text in case a bare code was encoded instead.
    private static string ExtractCode(string raw)
    {
        raw = raw.Trim();
        const string marker = "/by-code/";
        var i = raw.IndexOf(marker, StringComparison.OrdinalIgnoreCase);
        var code = i >= 0 ? raw[(i + marker.Length)..] : raw;
        return code.Split('/', '?', '#')[0].Trim();
    }
}

// String constants matching the API's CheckInResult values.
internal static class CheckInResultNames
{
    public const string CheckedIn = "CheckedIn";
    public const string AlreadyCheckedIn = "AlreadyCheckedIn";
    public const string NotPaid = "NotPaid";
    public const string Cancelled = "Cancelled";
    public const string TooEarly = "TooEarly";
    public const string Expired = "Expired";
    public const string NotFound = "NotFound";
}
