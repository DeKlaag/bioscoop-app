using System.Diagnostics;
using System.Security.Claims;
using Auth0.OidcClient;
using bioscoop_app.Services;

namespace bioscoop_app;

public partial class App : Application
{
    private readonly Auth0Client _auth0Client;
    private readonly IUserSession _session;

    public App(Auth0Client auth0Client, IUserSession session)
    {
        InitializeComponent();
        _auth0Client = auth0Client;
        _session = session;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        return new Window(new AppShell());
    }

    protected override async void OnStart()
    {
        base.OnStart();
        await TryRestoreSessionAsync();
    }

    private async Task TryRestoreSessionAsync()
    {
        var refreshToken = await _session.GetRefreshTokenAsync();
        if (string.IsNullOrEmpty(refreshToken))
        {
            Debug.WriteLine("[App] no stored refresh token; staying on login");
            return;
        }

        // Silent, browser-less re-authentication using the stored refresh token.
        var result = await _auth0Client.RefreshTokenAsync(refreshToken);
        if (result.IsError)
        {
            // Token revoked or expired - drop it and stay on the login screen.
            Debug.WriteLine($"[App] RefreshTokenAsync error: {result.Error}");
            _session.Clear();
            return;
        }

        // With rotation enabled a new refresh token is returned; persist it.
        await _session.SaveAsync(result.RefreshToken);

        // RefreshTokenResult carries tokens but no ClaimsPrincipal, so rebuild
        // the user from the userinfo endpoint to match the LoginAsync result.
        var userInfo = await _auth0Client.GetUserInfoAsync(result.AccessToken);
        if (userInfo.IsError)
        {
            Debug.WriteLine($"[App] GetUserInfoAsync error: {userInfo.Error}");
            return;
        }

        var identity = new ClaimsIdentity(userInfo.Claims, "Auth0", "name", "role");
        _session.User = new ClaimsPrincipal(identity);

        Debug.WriteLine("[App] silent login succeeded; navigating to MainPage");
        await Shell.Current.GoToAsync("//Tabs/MainPage");
    }
}
