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

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        await _auth0Client.LogoutAsync();
        _session.User = null;
        await Shell.Current.GoToAsync("//LoginPage");
    }
}
