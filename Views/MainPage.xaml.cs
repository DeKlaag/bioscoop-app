using Auth0.OidcClient;
using bioscoop_app.Services;

namespace bioscoop_app;

public partial class MainPage : ContentPage
{
    private readonly Auth0Client auth0Client;
    private readonly IUserSession session;

    public MainPage(Auth0Client client, IUserSession session)
    {
        InitializeComponent();
        auth0Client = client;
        this.session = session;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();

        var identity = session.User?.Identity;
        if (identity is not null && identity.IsAuthenticated)
        {
            UsernameLbl.Text = identity.Name;
        }
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        await auth0Client.LogoutAsync();
        session.User = null;
        await Shell.Current.GoToAsync("//LoginPage");
    }
}