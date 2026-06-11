using System.Diagnostics;
using System.Security.Claims;
using Auth0.OidcClient;
using bioscoop_app.Services;

namespace bioscoop_app;

public partial class App : Application
{
    private readonly Auth0Client _auth0Client;
    private readonly IUserSession _session;
    private readonly INotificationPreferencesService _prefs;
    private readonly INotificationScheduler _scheduler;
    private readonly IProximityService _proximity;
    private readonly IReservationService _reservationService;
    private readonly IReservationStore _store;

    public App(
        Auth0Client auth0Client,
        IUserSession session,
        INotificationPreferencesService prefs,
        INotificationScheduler scheduler,
        IProximityService proximity,
        IReservationService reservationService,
        IReservationStore store)
    {
        InitializeComponent();
        _auth0Client = auth0Client;
        _session = session;
        _prefs = prefs;
        _scheduler = scheduler;
        _proximity = proximity;
        _reservationService = reservationService;
        _store = store;
    }

    protected override Window CreateWindow(IActivationState? activationState)
    {
        var window = new Window(new AppShell());
        // Re-check reminders and proximity each time the app returns to the foreground.
        window.Resumed += (_, _) => _ = RefreshNotificationsAsync();
        return window;
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

        await RefreshNotificationsAsync();
    }

    // Re-schedules the 30-minute reminders and runs the foreground proximity check,
    // honouring the user's preferences. Best-effort and never throws into the caller.
    private async Task RefreshNotificationsAsync()
    {
        try
        {
            if (!_prefs.NotifyBeforeShow && !_prefs.NotifyOnArrival)
                return;

            var email = _session.User?.FindFirst("email")?.Value
                        ?? _session.User?.FindFirst(ClaimTypes.Email)?.Value;
            if (string.IsNullOrWhiteSpace(email))
                return; // only act when logged in

            // Cache-first so we still work offline; refresh in the background.
            var reservations = _store.Load(email);
            try
            {
                var fresh = await _reservationService.GetReservationsByEmailAsync(email);
                if (fresh.Count > 0)
                {
                    reservations = fresh;
                    _store.Save(email, fresh);
                }
            }
            catch
            {
                // Keep the cached list on network failure.
            }

            if (_prefs.NotifyBeforeShow)
                await _scheduler.RescheduleRemindersAsync(reservations);
            if (_prefs.NotifyOnArrival)
                await _proximity.CheckAsync(reservations);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[App] RefreshNotificationsAsync failed: {ex}");
        }
    }
}
