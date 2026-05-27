using bioscoop_app.ViewModels;

namespace bioscoop_app.Views;

public partial class MoviesPage : ContentPage
{
    private readonly MoviesViewModel _vm;

    public MoviesPage(MoviesViewModel vm)
    {
        InitializeComponent();
        BindingContext = _vm = vm;
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        if (_vm.Movies.Count == 0)
            await _vm.LoadCommand.ExecuteAsync(null);
    }
}
