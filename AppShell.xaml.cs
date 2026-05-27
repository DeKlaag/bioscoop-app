using System.Security.Claims;
using bioscoop_app.Services;
using Microsoft.Extensions.DependencyInjection;

namespace bioscoop_app;

public partial class AppShell : Shell
{
    public AppShell()
    {
        InitializeComponent();
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
}