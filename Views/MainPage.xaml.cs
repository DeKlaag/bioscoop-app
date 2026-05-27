using Auth0.OidcClient;
using bioscoop_app.Services;
using bioscoop_app.ViewModels;

namespace bioscoop_app;

public partial class MainPage : ContentPage
{
    private readonly Auth0Client auth0Client;
    private readonly IUserSession session;
    private readonly MoviesViewModel _vm;

    public MainPage(Auth0Client client, IUserSession session, MoviesViewModel moviesViewModel)
    {
        InitializeComponent();
        auth0Client = client;
        this.session = session;
        BindingContext = _vm = moviesViewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        var identity = session.User?.Identity;
        if (identity is not null && identity.IsAuthenticated)
        {
            UsernameLbl.Text = identity.Name;
        }
        
        if (_vm.Movies.Count == 0)
            await _vm.LoadCommand.ExecuteAsync(null);
    }

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        await auth0Client.LogoutAsync();
        session.User = null;
        await Shell.Current.GoToAsync("//LoginPage");
    }
}