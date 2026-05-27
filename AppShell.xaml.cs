using System.Security.Claims;
using bioscoop_app.Services;
using bioscoop_app.Views;
using Microsoft.Extensions.DependencyInjection;

namespace bioscoop_app;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
        Routing.RegisterRoute(nameof(ProfilePage), typeof(ProfilePage));
        Navigated += OnShellNavigated;
    }

    private void OnShellNavigated(object? sender, ShellNavigatedEventArgs e)
    {
        var session = IPlatformApplication.Current?.Services.GetService<IUserSession>();
        var identity = session?.User?.Identity;
        if (identity is not null && identity.IsAuthenticated)
        {
            UserPictureImg.Source = session!.User!
                .Claims.FirstOrDefault(c => c.Type == "picture")?.Value;
        }
        else
        {
            UserPictureImg.Source = null;
        }
    }

    private async void UserPictureImg_OnClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(nameof(ProfilePage));
    }
}