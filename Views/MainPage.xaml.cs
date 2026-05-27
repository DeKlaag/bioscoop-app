using bioscoop_app.ViewModels;

namespace bioscoop_app;

public partial class MainPage : ContentPage
{
    private readonly MoviesViewModel _vm;

    public MainPage(MoviesViewModel moviesViewModel)
    {
        InitializeComponent();
        BindingContext = _vm = moviesViewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_vm.Movies.Count == 0)
            await _vm.LoadCommand.ExecuteAsync(null);
    }
}
