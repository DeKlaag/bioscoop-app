using System.Diagnostics;
using System.Security.Claims;

namespace bioscoop_app.Services;

public interface IUserSession
{
    ClaimsPrincipal? User { get; set; }

    Task SaveAsync(string refreshToken);
    Task<string?> GetRefreshTokenAsync();
    void Clear();
}

public class UserSession : IUserSession
{
    private const string RefreshTokenKey = "auth0_refresh_token";

    public ClaimsPrincipal? User { get; set; }

    // SecureStorage (Keychain/Keystore) is the right home for a refresh token on
    // real devices. Some local desktop builds (e.g. ad-hoc signed Mac Catalyst
    // Debug) lack the keychain entitlement: SetAsync throws and GetAsync returns
    // null. So we mirror to Preferences and, on read, fall back to it whenever
    // SecureStorage yields nothing.
    public async Task SaveAsync(string refreshToken)
    {
        if (string.IsNullOrEmpty(refreshToken))
            return;

        try
        {
            await SecureStorage.Default.SetAsync(RefreshTokenKey, refreshToken);
            Debug.WriteLine("[UserSession] refresh token saved to SecureStorage");
        }
        catch (Exception ex)
        {
            Preferences.Default.Set(RefreshTokenKey, refreshToken);
            Debug.WriteLine($"[UserSession] SecureStorage failed ({ex.Message}); saved to Preferences");
        }
    }

    public async Task<string?> GetRefreshTokenAsync()
    {
        string? token = null;
        try
        {
            token = await SecureStorage.Default.GetAsync(RefreshTokenKey);
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[UserSession] SecureStorage read failed: {ex.Message}");
        }

        // Fall back to Preferences when SecureStorage has nothing (covers the
        // Mac Catalyst case where the token was mirrored there).
        if (string.IsNullOrEmpty(token))
            token = Preferences.Default.Get<string?>(RefreshTokenKey, null);

        Debug.WriteLine($"[UserSession] GetRefreshTokenAsync -> {(string.IsNullOrEmpty(token) ? "null" : "found")}");
        return token;
    }

    public void Clear()
    {
        User = null;

        try
        {
            SecureStorage.Default.Remove(RefreshTokenKey);
        }
        catch (Exception)
        {
            // ignore — Preferences cleared below regardless
        }

        Preferences.Default.Remove(RefreshTokenKey);
    }
}
