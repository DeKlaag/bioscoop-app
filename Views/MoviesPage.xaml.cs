using bioscoop_app.ViewModels;

namespace bioscoop_app.Views;

public partial class MoviesPage : ContentPage
{
    private readonly MoviesViewModel _vm;

    public MoviesPage(MoviesViewModel moviesViewModel)
    {
        InitializeComponent();
        BindingContext = _vm = moviesViewModel;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await _vm.LoadCommand.ExecuteAsync(null);
    }
}
