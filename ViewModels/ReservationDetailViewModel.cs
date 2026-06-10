using bioscoop_app.Models;
using bioscoop_app.Services;
using bioscoop_app.Views;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using QRCoder;

namespace bioscoop_app.ViewModels;

[QueryProperty(nameof(Reservation), "reservation")]
public partial class ReservationDetailViewModel : ObservableObject
{
    private readonly IReservationService _reservationService;

    public ReservationDetailViewModel(IReservationService reservationService)
        => _reservationService = reservationService;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanModify))]
    [NotifyPropertyChangedFor(nameof(StatusLabel))]
    [NotifyPropertyChangedFor(nameof(IsCancelled))]
    private ReservationModel? _reservation;

    [ObservableProperty]
    private bool _isBusy;

    // The reservation's QR code (encodes the by-code lookup URL). Shown on screen
    // and shared as a PNG.
    [ObservableProperty]
    private ImageSource? _qrImage;

    private byte[]? _qrBytes;
    private string? _qrCode;

    // Update (seats) and Cancel only make sense for an active, upcoming reservation.
    public bool CanModify => Reservation is { IsUpcoming: true, IsCancelled: false };

    public bool IsCancelled => Reservation?.IsCancelled ?? false;

    public string StatusLabel =>
        IsCancelled ? "Geannuleerd"
        : Reservation?.IsCheckedIn == true ? "Ingecheckt"
        : "Bevestigd";

    partial void OnReservationChanged(ReservationModel? value)
    {
        if (value?.PrintCode is { } code)
        {
            BuildQr(code);
            // Refresh from the API so the screen reflects the latest seats/status.
            _ = ReloadAsync(code);
        }
    }

    // Renders the reservation's QR code once per print code. The payload is the
    // by-code URL so a scan resolves straight to this reservation.
    private void BuildQr(string code)
    {
        if (_qrBytes is not null && _qrCode == code)
            return;

        using var generator = new QRCodeGenerator();
        using var data = generator.CreateQrCode(
            Constants.ReservationByCodeUrl(code), QRCodeGenerator.ECCLevel.Q);

        _qrBytes = new PngByteQRCode(data).GetGraphic(20);
        _qrCode = code;
        QrImage = ImageSource.FromStream(() => new MemoryStream(_qrBytes!));
    }

    // Re-fetches the reservation by its print code (called on appear and after edits).
    public async Task ReloadAsync(string? code = null)
    {
        code ??= Reservation?.PrintCode;
        if (string.IsNullOrWhiteSpace(code) || IsBusy)
            return;

        try
        {
            IsBusy = true;
            var fresh = await _reservationService.GetByCodeAsync(code);
            if (fresh is not null)
                Reservation = fresh;
        }
        catch
        {
            // Keep showing whatever we already have on a transient failure.
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ShareAsync()
    {
        if (Reservation?.PrintCode is not { } code)
            return;

        BuildQr(code);
        if (_qrBytes is null)
            return;

        // Share the QR as a PNG file so it can be saved/forwarded as an image.
        var path = Path.Combine(FileSystem.CacheDirectory, $"reservering-{code}.png");
        await File.WriteAllBytesAsync(path, _qrBytes);

        await Share.Default.RequestAsync(new ShareFileRequest
        {
            Title = "Deel reservering",
            File = new ShareFile(path),
        });
    }

    [RelayCommand]
    private async Task EditSeatsAsync()
    {
        if (Reservation is null || !CanModify)
            return;

        await Shell.Current.GoToAsync(nameof(EditSeatsPage),
            new Dictionary<string, object> { ["reservation"] = Reservation });
    }

    [RelayCommand]
    private async Task CancelAsync()
    {
        if (Reservation?.PrintCode is not { } code || !CanModify)
            return;

        var confirm = await Shell.Current.DisplayAlertAsync(
            "Reservering annuleren",
            "Weet je zeker dat je deze reservering wilt annuleren? De stoelen komen weer vrij.",
            "Ja, annuleer", "Nee");

        if (!confirm)
            return;

        try
        {
            IsBusy = true;
            var updated = await _reservationService.CancelAsync(code);
            if (updated is not null)
                Reservation = updated;
            else
                await Shell.Current.DisplayAlertAsync("Mislukt",
                    "Annuleren is niet gelukt. Probeer het later opnieuw.", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }
}
