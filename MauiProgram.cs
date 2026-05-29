using Microsoft.Extensions.Logging;
using Microsoft.Maui.Handlers;
using Auth0.OidcClient;
using bioscoop_app.Services;
using bioscoop_app.ViewModels;
using bioscoop_app.Views;

namespace bioscoop_app;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder
            .UseMauiApp<App>()
            .ConfigureFonts(fonts =>
            {
                fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
            });

#if IOS || MACCATALYST
        SearchBarHandler.Mapper.AppendToMapping("NoNativeBackground", (handler, _) =>
        {
            handler.PlatformView.BarTintColor = UIKit.UIColor.Clear;
            handler.PlatformView.BackgroundImage = new UIKit.UIImage();
            handler.PlatformView.SearchTextField.BackgroundColor = UIKit.UIColor.Clear;
        });
#endif

#if DEBUG
        builder.Logging.AddDebug();
#endif

        builder.Services.AddSingleton<MainPage>();
        builder.Services.AddSingleton<LoginPage>();
        builder.Services.AddSingleton<ProfilePage>();

        builder.Services.AddSingleton<IUserSession, UserSession>();
        builder.Services.AddSingleton<IMovieService, MovieService>();
        builder.Services.AddSingleton<MoviesViewModel>();
        builder.Services.AddSingleton<MoviesPage>();

        builder.Services.AddSingleton(new Auth0Client(new()
        {
            Domain = "dev-ffnab1idz1qjlenp.eu.auth0.com",
            ClientId = "ed1vutKp8l18pzEEbaa10j6XEFetQE8x",
            RedirectUri = "myapp://callback/",
            PostLogoutRedirectUri = "myapp://callback/",
            Scope = "openid profile email offline_access"
        }));
        
        return builder.Build();
    }
}