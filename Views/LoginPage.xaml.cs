using Auth0.OidcClient;
using bioscoop_app.Services;

namespace bioscoop_app.Views;

public partial class LoginPage : ContentPage
{
    private readonly Auth0Client _auth0Client;
    private readonly IUserSession _session;

    public LoginPage(Auth0Client client, IUserSession session)
    {
        InitializeComponent();
        _auth0Client = client;
        _session = session;
    }

    private async void OnLoginClicked(object sender, EventArgs e)
    {
        var result = await _auth0Client.LoginAsync();
        if (result.IsError)
        {
            await DisplayAlert("Error", result.ErrorDescription, "OK");
            return;
        }
        _session.User = result.User;
        await Shell.Current.GoToAsync("//Tabs/MainPage");
    }
}