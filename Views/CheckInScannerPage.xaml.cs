using bioscoop_app.ViewModels;
using BarcodeScanning;

namespace bioscoop_app.Views;

public partial class CheckInScannerPage : ContentPage
{
    private readonly CheckInScannerViewModel _vm;

    public CheckInScannerPage(CheckInScannerViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
        _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
        if (status != PermissionStatus.Granted)
            status = await Permissions.RequestAsync<Permissions.Camera>();

        if (status == PermissionStatus.Granted)
        {
            // Enabling this starts the camera (CameraView.CameraEnabled binds to it).
            _vm.CameraAuthorized = true;
        }
        else
        {
            await DisplayAlertAsync("Camera",
                "Geef toegang tot de camera om QR-codes te kunnen scannen.", "OK");
            await Shell.Current.GoToAsync("..");
        }
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        // Stop the camera when leaving the page.
        _vm.CameraAuthorized = false;
    }

    private void OnDetectionFinished(object? sender, OnDetectionFinishedEventArg e)
    {
        var value = e.BarcodeResults?.FirstOrDefault()?.DisplayValue;
        if (string.IsNullOrWhiteSpace(value))
            return;

        // The event fires off the UI thread; marshal before touching the view model.
        Dispatcher.Dispatch(async () => await _vm.HandleScanAsync(value));
    }
}
