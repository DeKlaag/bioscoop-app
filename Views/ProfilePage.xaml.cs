using Auth0.OidcClient;
using bioscoop_app.Services;

namespace bioscoop_app.Views;

public partial class ProfilePage
{
    private readonly Auth0Client _auth0Client;
    private readonly IUserSession _session;

    public ProfilePage(Auth0Client client, IUserSession session)
    {
        InitializeComponent();
        _auth0Client = client;
        _session = session;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        var user = _session.User;
        if (user?.Identity is { IsAuthenticated: true })
        {
            UsernameLbl.Text = user.Identity.Name ?? string.Empty;
            UserPictureImg.Source = user.Claims
                .FirstOrDefault(c => c.Type == "picture")?.Value;
        }
    }

    private async void OnCheckInScannerClicked(object sender, EventArgs e)
    {
        // Ask for camera access BEFORE opening the scanner page, so the camera view
        // is only ever created once permission is granted (creating it without
        // permission / usage description crashes on iOS).
        var status = await Permissions.CheckStatusAsync<Permissions.Camera>();
        if (status != PermissionStatus.Granted)
            status = await Permissions.RequestAsync<Permissions.Camera>();

        if (status != PermissionStatus.Granted)
        {
            await DisplayAlertAsync("Camera",
                "Geef toegang tot de camera om QR-codes te kunnen scannen.", "OK");
            return;
        }

        await Shell.Current.GoToAsync(nameof(CheckInScannerPage));
    }

    private async void OnNotificationPreferencesClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(NotificationPreferencesPage));
    }

    private async void OnFeedbackClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(FeedbackPage));
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        await _auth0Client.LogoutAsync();
        _session.Clear();
        await Shell.Current.GoToAsync("//LoginPage");
    }
}
